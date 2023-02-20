using QueryBuilder.Clauses;
using QueryBuilder.Exceptions;
using System.Collections.Concurrent;
using System.Reflection;

namespace Zine.QueryBuilder;

public partial class Query : BaseQuery<Query>
{
    private string? _comment;

    public bool IsDistinct { get; set; }
    public string? QueryAlias { get; set; }
    public string Method { get; set; } = "select";
    public Dictionary<string, object> Variables = new();

    public Query() : base()
    {
    }

    public Query(string table, string? comment = null) : base()
    {
        From(table);
        Comment(comment);
    }

    public string GetComment() => _comment ?? "";

    public bool HasOffset(string? engineCode = null) => GetOffset(engineCode) > 0;

    public bool HasLimit(string? engineCode = null) => GetLimit(engineCode) > 0;

    internal long GetOffset(string? engineCode = null)
    {
        engineCode ??= EngineScope;
        var offset = GetOneComponent<OffsetClause>(Component.Offset, engineCode);

        return offset?.Offset ?? 0;
    }

    internal int GetLimit(string? engineCode = null)
    {
        engineCode ??= EngineScope;
        var limit = GetOneComponent<LimitClause>(Component.Limit, engineCode);

        return limit?.Limit ?? 0;
    }

    public override Query Clone()
    {
        var clone = base.Clone();
        clone.Parent = Parent;
        clone.QueryAlias = QueryAlias;
        clone.IsDistinct = IsDistinct;
        clone.Method = Method;
        clone.Variables = Variables;
        return clone;
    }

    public Query As(string alias)
    {
        QueryAlias = alias;
        return this;
    }

    /// <summary>
    /// Sets a comment for the query.
    /// </summary>
    /// <param name="comment">The comment.</param>
    /// <returns></returns>
    public Query Comment(string? comment)
    {
        _comment = comment;
        return this;
    }

    public Query For(string engine, Func<Query, Query> fn)
    {
        EngineScope = engine;

        var result = fn.Invoke(this);

        // reset the engine
        EngineScope = null;

        return result;
    }

    public Query With(Query query)
    {
        // Clear query alias and add it to the containing clause
        if (string.IsNullOrWhiteSpace(query.QueryAlias))
        {
            throw new InvalidOperationException("No Alias found for the CTE query");
        }

        query = query.Clone();

        CustomNullReferenceException.ThrowIfNull(query.QueryAlias);

        var alias = query.QueryAlias.Trim();

        // clear the query alias
        query.QueryAlias = null;

        return AddComponent(Component.Cte, new QueryFromClause
        {
            Query = query,
            Alias = alias,
        });
    }

    public Query With(Func<Query, Query> fn)
        => With(fn.Invoke(new Query()));

    public Query With(string alias, Query query)
        => With(query.As(alias));

    public Query With(string alias, Func<Query, Query> fn)
        => With(alias, fn.Invoke(new Query()));

    /// <summary>
    /// Constructs an ad-hoc table of the given data as a CTE.
    /// </summary>
    public Query With(string alias, IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection)
    {
        ArgumentNullException.ThrowIfNull(alias);
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(valuesCollection);

        var columnsList = columns.ToList();
        var valuesCollectionList = valuesCollection.ToList();

        if (columnsList.Count == 0 || valuesCollectionList.Count == 0)
        {
            throw new InvalidOperationException("Columns and valuesCollection cannot be null or empty");
        }

        var clause = new AdHocTableFromClause()
        {
            Alias = alias,
            Columns = columnsList,
            Values = new List<object>(),
        };

        foreach (var values in valuesCollectionList)
        {
            var valuesList = values.ToList();
            if (columnsList.Count != valuesList.Count)
            {
                throw new InvalidOperationException("Columns count should be equal to each Values count");
            }

            clause.Values.AddRange(valuesList);
        }

        return AddComponent(Component.Cte, clause);
    }

    public Query WithRaw(string alias, string sql, params object[] bindings)
        => AddComponent(Component.Cte, new RawFromClause
        {
            Alias = alias,
            Expression = sql,
            Bindings = bindings,
        });

    public Query Limit(int value)
    {
        var newClause = new LimitClause
        {
            Limit = value
        };

        return AddOrReplaceComponent(Component.Limit, newClause);
    }

    public Query Offset(long value)
    {
        var newClause = new OffsetClause
        {
            Offset = value
        };

        return AddOrReplaceComponent(Component.Offset, newClause);
    }

    public Query Offset(int value)
        => Offset((long)value);

    /// <summary>
    /// Alias for Limit
    /// </summary>
    /// <param name="limit"></param>
    /// <returns></returns>
    public Query Take(int limit)
        => Limit(limit);

    /// <summary>
    /// Alias for Offset
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public Query Skip(int offset)
        => Offset(offset);

    public Query Distinct()
    {
        IsDistinct = true;
        return this;
    }

    /// <summary>
    /// Apply the callback's query changes if the given "condition" is true.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="whenTrue">Invoked when the condition is true</param>
    /// <param name="whenFalse">Optional, invoked when the condition is false</param>
    /// <returns></returns>
    public Query When(bool condition, Func<Query, Query> whenTrue, Func<Query, Query>? whenFalse = null)
    {
        if (condition && whenTrue != null)
        {
            return whenTrue.Invoke(this);
        }

        if (!condition && whenFalse != null)
        {
            return whenFalse.Invoke(this);
        }

        return this;
    }

    /// <summary>
    /// Apply the callback's query changes if the given "condition" is false.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public Query WhenNot(bool condition, Func<Query, Query> callback)
        => condition ? this : callback.Invoke(this);

    public Query OrderBy(params string[] columns)
    {
        foreach (var column in columns)
        {
            AddComponent(Component.Order, new OrderBy
            {
                Column = column,
                Ascending = true
            });
        }

        return this;
    }

    public Query OrderByDesc(params string[] columns)
    {
        foreach (var column in columns)
        {
            AddComponent(Component.Order, new OrderBy
            {
                Column = column,
                Ascending = false
            });
        }

        return this;
    }

    public Query OrderByRaw(string expression, params object[] bindings)
        => AddComponent(Component.Order,
            new RawOrderBy
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });

    public Query OrderByRandom()
        => AddComponent(Component.Order, new OrderByRandom { });

    public Query GroupBy(params string[] columns)
    {
        foreach (var column in columns)
        {
            AddComponent(Component.Group, new Column
            {
                Name = column
            });
        }

        return this;
    }

    public Query GroupByRaw(string expression, params object[] bindings)
    {
        AddComponent(Component.Group, new RawColumn
        {
            Expression = expression,
            Bindings = bindings,
        });

        return this;
    }

    public override Query NewQuery()
        => new();

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> CacheDictionaryProperties = new();

    /// <summary>
    /// Define a variable to be used within the query
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Query Define(string variable, object value)
    {
        Variables.Add(variable, value);

        return this;
    }

    public object FindVariable(string variable)
    {
        var found = Variables.ContainsKey(variable);

        if (found)
        {
            return Variables[variable];
        }

        if (Parent is Query parent)
        {
            return parent.FindVariable(variable);
        }

        throw new Exception($"Variable '{variable}' not found");
    }
}
