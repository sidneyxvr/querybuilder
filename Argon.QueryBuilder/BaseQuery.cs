using Argon.QueryBuilder.Clauses;
using System.Text.RegularExpressions;

namespace Argon.QueryBuilder;

public abstract class AbstractQuery
{
    public AbstractQuery? Parent;
}

public abstract partial class BaseQuery<Q> : AbstractQuery where Q : BaseQuery<Q>
{
    [GeneratedRegex(@"^(?:\w+\.){1,2}{(.*)}")]
    private static partial Regex ExpandRegex();


    [GeneratedRegex("\\s*,\\s*")]
    private static partial Regex ColumnRegex();


    [GeneratedRegex(" as ", RegexOptions.IgnoreCase)]
    private static partial Regex AliasRegex();

    public List<AbstractColumn> Columns { get; set; } = new();
    public List<AggregateClause> AggregateColumns { get; set; } = new();
    public AbstractFrom? FromClause { get; set; }
    public List<BaseJoin> Joins { get; set; } = new();
    public List<AbstractCondition> Conditions { get; set; } = new();
    public AbstractCondition? HavingClause { get; set; }
    public List<AbstractOrderBy> OrderByColumns { get; set; } = new();
    public List<AbstractColumn> GroupByColumns { get; set; } = new();
    public List<AbstractCombine> Unions{ get; set; } = new();
    public LimitClause? LimitClause { get; set; }
    public OffsetClause? OffsetClause { get; set; }


    private bool orFlag = false;
    private bool notFlag = false;

    public BaseQuery()
    {
    }

    /// <summary>
    /// Return a cloned copy of the current query.
    /// </summary>
    /// <returns></returns>
    public virtual Q Clone()
    {
        var q = NewQuery();

        return q;
    }

    public Q SetParent(AbstractQuery parent)
    {
        if (this == parent)
        {
            throw new ArgumentException($"Cannot set the same {nameof(AbstractQuery)} as a parent of itself");
        }

        Parent = parent;
        return (Q)this;
    }

    public abstract Q NewQuery();

    public Q NewChild()
    {
        var newQuery = NewQuery().SetParent((Q)this);
        return newQuery;
    }

    /// <summary>
    /// Add a component clause to the query.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="clause"></param>
    /// <param name="engineCode"></param>
    /// <returns></returns>
    public Q AddComponent(ComponentType component, AbstractClause clause)
    {
        clause.Component = component;
        if (component == ComponentType.Select)
        {
            Columns.Add((AbstractColumn)clause);
        }
        else if (component == ComponentType.From)
        {
            FromClause = (AbstractFrom)clause;
        }
        else if (component == ComponentType.Join)
        {
            Joins.Add((BaseJoin)clause);
        }
        else if (component == ComponentType.Where)
        {
            Conditions.Add((AbstractCondition)clause);
        }
        else if (component == ComponentType.Order)
        {
            OrderByColumns.Add((AbstractOrderBy)clause);
        }
        else if (component == ComponentType.Group)
        {
            GroupByColumns.Add((AbstractColumn)clause);
        }
        else if (component == ComponentType.Limit)
        {
            LimitClause = (LimitClause)clause;
        }
        else if (component == ComponentType.Offset)
        {
            OffsetClause = (OffsetClause)clause;
        }
        else if (component == ComponentType.Aggregate)
        {
            AggregateColumns.Add((AggregateClause)clause);
        }
        else if (component == ComponentType.Union)
        {
            Unions.Add((AbstractCombine)clause);
        }

        return (Q)this;
    }

    /// <summary>
    /// If the query already contains a clause for the given component
    /// and engine, replace it with the specified clause. Otherwise, just
    /// add the clause.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="clause"></param>
    /// <param name="engineCode"></param>
    /// <returns></returns>
    public Q AddOrReplaceComponent<C>(ComponentType component, AbstractClause clause)
        where C : AbstractClause
        => AddComponent(component, clause);

    /// <summary>
    /// Set the next boolean operator to "and" for the "where" clause.
    /// </summary>
    /// <returns></returns>
    protected Q And()
    {
        orFlag = false;
        return (Q)this;
    }

    /// <summary>
    /// Set the next boolean operator to "or" for the "where" clause.
    /// </summary>
    /// <returns></returns>
    public Q Or()
    {
        orFlag = true;
        return (Q)this;
    }

    /// <summary>
    /// Set the next "not" operator for the "where" clause.
    /// </summary>
    /// <returns></returns>
    public Q Not(bool flag = true)
    {
        notFlag = flag;
        return (Q)this;
    }

    /// <summary>
    /// Get the boolean operator and reset it to "and"
    /// </summary>
    /// <returns></returns>
    protected bool GetOr()
    {
        var ret = orFlag;

        // reset the flag
        orFlag = false;
        return ret;
    }

    /// <summary>
    /// Get the "not" operator and clear it
    /// </summary>
    /// <returns></returns>
    protected bool GetNot()
    {
        var ret = notFlag;

        // reset the flag
        notFlag = false;
        return ret;
    }

    /// <summary>
    /// Add a from Clause
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public Q From(string table)
    {
        var (name, alias) = ExpandTableExpression(table);

        return AddOrReplaceComponent<FromClause>(ComponentType.From,
        new FromClause
        {
            Table = name,
            Alias = alias
        });
    }

    public Q From(Query query, string? alias = null)
    {
        query = query.Clone();
        query.SetParent((Q)this);

        if (alias != null)
        {
            query.As(alias);
        };

        return AddOrReplaceComponent<QueryFromClause>(ComponentType.From, new QueryFromClause
        {
            Query = query
        });
    }

    public Q FromRaw(string sql, params object[] bindings)
        => AddOrReplaceComponent<RawFromClause>(ComponentType.From, new RawFromClause
        {
            Expression = sql,
            Bindings = bindings,
        });

    public Q From(Func<Query, Query> callback, string? alias = null)
    {
        var query = new Query();

        query.SetParent((Q)this);

        return From(callback.Invoke(query), alias);
    }
}
