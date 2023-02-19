using QueryBuilder.Clauses;
using QueryBuilder.Exceptions;
using System.Reflection;

namespace SqlKata;

public abstract partial class BaseQuery<Q>
{
    public Q Where(string column, string op, object value)
    {
        // If the value is "null", we will just assume the developer wants to add a
        // where null clause to the query. So, we will allow a short-cut here to
        // that method for convenience so the developer doesn't have to check.
        if (value == null)
        {
            return Not(op != "=").WhereNull(column);
        }

        if (value is bool boolValue)
        {
            if (op != "=")
            {
                Not();
            }

            return boolValue ? WhereTrue(column) : WhereFalse(column);
        }

        return AddComponent(Component.Where, new BasicCondition
        {
            Column = column,
            Operator = op,
            Value = value,
            IsOr = GetOr(),
            IsNot = GetNot(),
        });
    }

    public Q WhereNot(string column, string op, object value)
        => Not().Where(column, op, value);

    public Q OrWhere(string column, string op, object value)
        => Or().Where(column, op, value);

    public Q OrWhereNot(string column, string op, object value)
        => Or().Not().Where(column, op, value);

    public Q Where(string column, object value)
        => Where(column, "=", value);

    public Q WhereNot(string column, object value)
        => WhereNot(column, "=", value);

    public Q OrWhere(string column, object value)
        => OrWhere(column, "=", value);

    public Q OrWhereNot(string column, object value)
        => OrWhereNot(column, "=", value);

    /// <summary>
    /// Perform a where constraint
    /// </summary>
    /// <param name="constraints"></param>
    /// <returns></returns>
    public Q Where(object constraints)
    {
        var dictionary = new Dictionary<string, object>();

        foreach (var item in constraints.GetType().GetRuntimeProperties())
        {
            var currentItem = item.GetValue(constraints);

            CustomNullReferenceException.ThrowIfNull(currentItem);

            dictionary.Add(item.Name, currentItem);
        }

        return Where(dictionary);
    }

    public Q Where(IEnumerable<KeyValuePair<string, object>> values)
    {
        var query = (Q)this;
        var orFlag = GetOr();
        var notFlag = GetNot();

        foreach (var tuple in values)
        {
            if (orFlag)
            {
                query = query.Or();
            }
            else
            {
                query.And();
            }

            query = Not(notFlag).Where(tuple.Key, tuple.Value);
        }

        return query;
    }

    public Q WhereRaw(string sql, params object[] bindings)
        => AddComponent(Component.Where,
        new RawCondition
        {
            Expression = sql,
            Bindings = bindings,
            IsOr = GetOr(),
            IsNot = GetNot(),
        });

    public Q OrWhereRaw(string sql, params object[] bindings)
        => Or().WhereRaw(sql, bindings);

    /// <summary>
    /// Apply a nested where clause
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public Q Where(Func<Q, Q> callback)
    {
        var query = callback.Invoke(NewChild());

        // omit empty queries
        if (!query.Clauses.Where(x => x.Component == Component.Where).Any())
        {
            return (Q)this;
        }

        return AddComponent(Component.Where, new NestedCondition<Q>
        {
            Query = query,
            IsNot = GetNot(),
            IsOr = GetOr(),
        });
    }

    public Q WhereNot(Func<Q, Q> callback)
        => Not().Where(callback);

    public Q OrWhere(Func<Q, Q> callback)
        => Or().Where(callback);

    public Q OrWhereNot(Func<Q, Q> callback)
        => Not().Or().Where(callback);

    public Q WhereColumns(string first, string op, string second)
        => AddComponent(Component.Where,
        new TwoColumnsCondition
        {
            First = first,
            Second = second,
            Operator = op,
            IsOr = GetOr(),
            IsNot = GetNot(),
        });

    public Q OrWhereColumns(string first, string op, string second)
        => Or().WhereColumns(first, op, second);

    public Q WhereNull(string column)
        => AddComponent(Component.Where,
        new NullCondition
        {
            Column = column,
            IsOr = GetOr(),
            IsNot = GetNot()
        });

    public Q WhereNotNull(string column)
        => Not().WhereNull(column);

    public Q OrWhereNull(string column)
        => Or().WhereNull(column);

    public Q OrWhereNotNull(string column)
        => Or().Not().WhereNull(column);

    public Q WhereTrue(string column)
        => AddComponent(Component.Where,
        new BooleanCondition
        {
            Column = column,
            Value = true,
            IsOr = GetOr(),
            IsNot = GetNot()
        });

    public Q OrWhereTrue(string column)
        => Or().WhereTrue(column);

    public Q WhereFalse(string column)
        => AddComponent(Component.Where,
        new BooleanCondition
        {
            Column = column,
            Value = false,
            IsOr = GetOr(),
            IsNot = GetNot()
        });

    public Q OrWhereFalse(string column)
        => Or().WhereFalse(column);

    public Q WhereLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => AddComponent(Component.Where,
        new BasicStringCondition
        {
            Operator = "like",
            Column = column,
            Value = value,
            CaseSensitive = caseSensitive,
            EscapeCharacter = escapeCharacter,
            IsOr = GetOr(),
            IsNot = GetNot(),
        });

    public Q WhereNotLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Not().WhereLike(column, value, caseSensitive, escapeCharacter);

    public Q OrWhereLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().WhereLike(column, value, caseSensitive, escapeCharacter);

    public Q OrWhereNotLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().Not().WhereLike(column, value, caseSensitive, escapeCharacter);

    public Q WhereStarts(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => AddComponent(Component.Where,
        new BasicStringCondition
        {
            Operator = "starts",
            Column = column,
            Value = value,
            CaseSensitive = caseSensitive,
            EscapeCharacter = escapeCharacter,
            IsOr = GetOr(),
            IsNot = GetNot(),
        });

    public Q WhereNotStarts(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Not().WhereStarts(column, value, caseSensitive, escapeCharacter);

    public Q OrWhereStarts(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().WhereStarts(column, value, caseSensitive, escapeCharacter);

    public Q OrWhereNotStarts(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().Not().WhereStarts(column, value, caseSensitive, escapeCharacter);

    public Q WhereEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => AddComponent(Component.Where,
        new BasicStringCondition
        {
            Operator = "ends",
            Column = column,
            Value = value,
            CaseSensitive = caseSensitive,
            EscapeCharacter = escapeCharacter,
            IsOr = GetOr(),
            IsNot = GetNot()
        });

    public Q WhereNotEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Not().WhereEnds(column, value, caseSensitive, escapeCharacter);

    public Q OrWhereEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().WhereEnds(column, value, caseSensitive, escapeCharacter);

    public Q OrWhereNotEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().Not().WhereEnds(column, value, caseSensitive, escapeCharacter);

    public Q WhereContains(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => AddComponent(Component.Where,
        new BasicStringCondition
        {
            Operator = "contains",
            Column = column,
            Value = value,
            CaseSensitive = caseSensitive,
            EscapeCharacter = escapeCharacter,
            IsOr = GetOr(),
            IsNot = GetNot()
        });

    public Q WhereNotContains(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Not().WhereContains(column, value, caseSensitive, escapeCharacter);

    public Q OrWhereContains(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().WhereContains(column, value, caseSensitive, escapeCharacter);

    public Q OrWhereNotContains(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().Not().WhereContains(column, value, caseSensitive, escapeCharacter);

    public Q WhereBetween<T>(string column, T lower, T higher)
        where T : notnull
        => AddComponent(Component.Where, new BetweenCondition<T>
        {
            Column = column,
            IsOr = GetOr(),
            IsNot = GetNot(),
            Lower = lower,
            Higher = higher
        });

    public Q OrWhereBetween<T>(string column, T lower, T higher)
        where T : notnull
        => Or().WhereBetween(column, lower, higher);

    public Q WhereNotBetween<T>(string column, T lower, T higher)
        where T : notnull
        => Not().WhereBetween(column, lower, higher);

    public Q OrWhereNotBetween<T>(string column, T lower, T higher)
        where T : notnull
        => Or().Not().WhereBetween(column, lower, higher);

    public Q WhereIn<T>(string column, IEnumerable<T> values)
    {
        // If the developer has passed a string they most likely want a List<string>
        // since string is considered as List<char>
        if (values is string value)
        {
            return AddComponent(Component.Where, new InCondition<string>
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Values = new List<string> { value }
            });
        }

        return AddComponent(Component.Where, new InCondition<T>
        {
            Column = column,
            IsOr = GetOr(),
            IsNot = GetNot(),
            Values = values.Distinct().ToList()
        });
    }

    public Q OrWhereIn<T>(string column, IEnumerable<T> values)
        => Or().WhereIn(column, values);

    public Q WhereNotIn<T>(string column, IEnumerable<T> values)
        => Not().WhereIn(column, values);

    public Q OrWhereNotIn<T>(string column, IEnumerable<T> values)
        => Or().Not().WhereIn(column, values);

    public Q WhereIn(string column, Query query)
        => AddComponent(Component.Where,
        new InQueryCondition
        {
            Column = column,
            IsOr = GetOr(),
            IsNot = GetNot(),
            Query = query,
        });

    public Q WhereIn(string column, Func<Query, Query> callback)
    {
        var query = callback.Invoke(new Query().SetParent(this));

        return WhereIn(column, query);
    }

    public Q OrWhereIn(string column, Query query)
        => Or().WhereIn(column, query);

    public Q OrWhereIn(string column, Func<Query, Query> callback)
        => Or().WhereIn(column, callback);

    public Q WhereNotIn(string column, Query query)
        => Not().WhereIn(column, query);

    public Q WhereNotIn(string column, Func<Query, Query> callback)
        => Not().WhereIn(column, callback);

    public Q OrWhereNotIn(string column, Query query)
        => Or().Not().WhereIn(column, query);

    public Q OrWhereNotIn(string column, Func<Query, Query> callback)
        => Or().Not().WhereIn(column, callback);

    /// <summary>
    /// Perform a sub query where clause
    /// </summary>
    /// <param name="column"></param>
    /// <param name="op"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public Q Where(string column, string op, Func<Q, Q> callback)
    {
        var query = callback.Invoke(NewChild());

        return Where(column, op, query);
    }

    public Q Where(string column, string op, Query query)
        => AddComponent(Component.Where,
        new QueryCondition<Query>
        {
            Column = column,
            Operator = op,
            Query = query,
            IsNot = GetNot(),
            IsOr = GetOr()
        });

    public Q WhereSub(Query query, object value)
        => WhereSub(query, "=", value);

    public Q WhereSub(Query query, string op, object value)
        => AddComponent(Component.Where,
        new SubQueryCondition<Query>
        {
            Value = value,
            Operator = op,
            Query = query,
            IsNot = GetNot(),
            IsOr = GetOr()
        });

    public Q OrWhereSub(Query query, object value)
        => Or().WhereSub(query, value);

    public Q OrWhereSub(Query query, string op, object value)
        => Or().WhereSub(query, op, value);

    public Q OrWhere(string column, string op, Query query)
        => Or().Where(column, op, query);
    public Q OrWhere(string column, string op, Func<Query, Query> callback)
        => Or().Where(column, op, callback);

    public Q WhereExists(Query query)
    {
        if (!query.HasComponent(Component.From))
        {
            throw new ArgumentException($"'{nameof(FromClause)}' cannot be empty if used inside a '{nameof(WhereExists)}' condition");
        }

        return AddComponent(Component.Where, new ExistsCondition
        {
            Query = query,
            IsNot = GetNot(),
            IsOr = GetOr(),
        });
    }
    public Q WhereExists(Func<Query, Query> callback)
    {
        var childQuery = new Query().SetParent(this);
        return WhereExists(callback.Invoke(childQuery));
    }

    public Q WhereNotExists(Query query)
        => Not().WhereExists(query);

    public Q WhereNotExists(Func<Query, Query> callback)
        => Not().WhereExists(callback);

    public Q OrWhereExists(Query query)
        => Or().WhereExists(query);

    public Q OrWhereExists(Func<Query, Query> callback)
        => Or().WhereExists(callback);

    public Q OrWhereNotExists(Query query)
        => Or().Not().WhereExists(query);

    public Q OrWhereNotExists(Func<Query, Query> callback)
        => Or().Not().WhereExists(callback);

    public Q WhereDatePart(string part, string column, string op, object value)
        => AddComponent(Component.Where,
         new BasicDateCondition
         {
             Operator = op,
             Column = column,
             Value = value,
             Part = part.ToLowerInvariant(),
             IsOr = GetOr(),
             IsNot = GetNot()
         });

    public Q WhereNotDatePart(string part, string column, string op, object value)
        => Not().WhereDatePart(part, column, op, value);

    public Q OrWhereDatePart(string part, string column, string op, object value)
        => Or().WhereDatePart(part, column, op, value);

    public Q OrWhereNotDatePart(string part, string column, string op, object value)
        => Or().Not().WhereDatePart(part, column, op, value);

    public Q WhereDate(string column, string op, object value)
        => WhereDatePart("date", column, op, value);

    public Q WhereNotDate(string column, string op, object value)
        => Not().WhereDate(column, op, value);

    public Q OrWhereDate(string column, string op, object value)
        => Or().WhereDate(column, op, value);

    public Q OrWhereNotDate(string column, string op, object value)
        => Or().Not().WhereDate(column, op, value);

    public Q WhereTime(string column, string op, object value)
        => WhereDatePart("time", column, op, value);

    public Q WhereNotTime(string column, string op, object value)
        => Not().WhereTime(column, op, value);

    public Q OrWhereTime(string column, string op, object value)
        => Or().WhereTime(column, op, value);

    public Q OrWhereNotTime(string column, string op, object value)
        => Or().Not().WhereTime(column, op, value);

    public Q WhereDatePart(string part, string column, object value)
        => WhereDatePart(part, column, "=", value);

    public Q WhereNotDatePart(string part, string column, object value)
        => WhereNotDatePart(part, column, "=", value);

    public Q OrWhereDatePart(string part, string column, object value)
        => OrWhereDatePart(part, column, "=", value);

    public Q OrWhereNotDatePart(string part, string column, object value)
        => OrWhereNotDatePart(part, column, "=", value);

    public Q WhereDate(string column, object value)
        => WhereDate(column, "=", value);

    public Q WhereNotDate(string column, object value)
        => WhereNotDate(column, "=", value);

    public Q OrWhereDate(string column, object value)
        => OrWhereDate(column, "=", value);

    public Q OrWhereNotDate(string column, object value)
        => OrWhereNotDate(column, "=", value);

    public Q WhereTime(string column, object value)
        => WhereTime(column, "=", value);

    public Q WhereNotTime(string column, object value)
        => WhereNotTime(column, "=", value);

    public Q OrWhereTime(string column, object value)
        => OrWhereTime(column, "=", value);

    public Q OrWhereNotTime(string column, object value)
        => OrWhereNotTime(column, "=", value);
}
