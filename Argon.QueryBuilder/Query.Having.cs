using System.Reflection;
using Argon.QueryBuilder.Clauses;
using Argon.QueryBuilder.Exceptions;

namespace Argon.QueryBuilder;

public partial class Query
{
    public Query Having(string column, string op, object value)
    {
        // If the value is "null", we will just assume the developer wants to add a
        // Having null clause to the query. So, we will allow a short-cut here to
        // that method for convenience so the developer doesn't have to check.
        if (value == null)
        {
            return Not(op != "=").HavingNull(column);
        }

        return AddComponent(Component.Having, new BasicCondition
        {
            Column = column,
            Operator = op,
            Value = value,
            IsOr = GetOr(),
            IsNot = GetNot(),
        });
    }

    public Query HavingNot(string column, string op, object value)
        => Not().Having(column, op, value);

    public Query OrHaving(string column, string op, object value)
        => Or().Having(column, op, value);

    public Query OrHavingNot(string column, string op, object value)
        => Or().Not().Having(column, op, value);

    public Query Having(string column, object value)
        => Having(column, "=", value);

    public Query HavingNot(string column, object value)
        => HavingNot(column, "=", value);
    public Query OrHaving(string column, object value)
        => OrHaving(column, "=", value);

    public Query OrHavingNot(string column, object value)
        => OrHavingNot(column, "=", value);

    /// <summary>
    /// Perform a Having constraint
    /// </summary>
    /// <param name="constraints"></param>
    /// <returns></returns>
    public Query Having(object constraints)
    {
        ArgumentNullException.ThrowIfNull(constraints);

        var dictionary = new Dictionary<string, object>();

        foreach (var item in constraints.GetType().GetRuntimeProperties())
        {
            var currentItem = item.GetValue(constraints);

            CustomNullReferenceException.ThrowIfNull(currentItem);

            dictionary.Add(item.Name, currentItem);
        }

        return Having(dictionary);
    }

    public Query Having(IEnumerable<KeyValuePair<string, object>> values)
    {
        var query = this;
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

            query = Not(notFlag).Having(tuple.Key, tuple.Value);
        }

        return query;
    }

    public Query HavingRaw(string sql, params object[] bindings)
        => AddComponent(Component.Having, new RawCondition
        {
            Expression = sql,
            Bindings = bindings,
            IsOr = GetOr(),
            IsNot = GetNot(),
        });

    public Query OrHavingRaw(string sql, params object[] bindings)
        => Or().HavingRaw(sql, bindings);

    /// <summary>
    /// Apply a nested Having clause
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public Query Having(Func<Query, Query> callback)
    {
        var query = callback.Invoke(NewChild());

        return AddComponent(Component.Having, new NestedCondition<Query>
        {
            Query = query,
            IsNot = GetNot(),
            IsOr = GetOr(),
        });
    }

    public Query HavingNot(Func<Query, Query> callback)
        => Not().Having(callback);

    public Query OrHaving(Func<Query, Query> callback)
        => Or().Having(callback);

    public Query OrHavingNot(Func<Query, Query> callback)
        => Not().Or().Having(callback);

    public Query HavingColumns(string first, string op, string second)
        => AddComponent(Component.Having, new TwoColumnsCondition { First = first, Second = second, Operator = op, IsOr = GetOr(), IsNot = GetNot() });

    public Query OrHavingColumns(string first, string op, string second)
        => Or().HavingColumns(first, op, second);

    public Query HavingNull(string column)
        => AddComponent(Component.Having, new NullCondition { Column = column, IsOr = GetOr(), IsNot = GetNot() });

    public Query HavingNotNull(string column)
        => Not().HavingNull(column);

    public Query OrHavingNull(string column)
        => Or().HavingNull(column);

    public Query OrHavingNotNull(string column)
        => Or().Not().HavingNull(column);

    public Query HavingTrue(string column)
        => AddComponent(Component.Having, new BooleanCondition { Column = column, Value = true });

    public Query OrHavingTrue(string column)
        => Or().HavingTrue(column);

    public Query HavingFalse(string column)
        => AddComponent(Component.Having, new BooleanCondition { Column = column, Value = false });

    public Query OrHavingFalse(string column)
        => Or().HavingFalse(column);

    public Query HavingLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => AddComponent(Component.Having, new BasicStringCondition { Operator = "like", Column = column, Value = value, CaseSensitive = caseSensitive, EscapeCharacter = escapeCharacter, IsOr = GetOr(), IsNot = GetNot() });

    public Query HavingNotLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Not().HavingLike(column, value, caseSensitive, escapeCharacter);

    public Query OrHavingLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().HavingLike(column, value, caseSensitive, escapeCharacter);

    public Query OrHavingNotLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().Not().HavingLike(column, value, caseSensitive, escapeCharacter);

    public Query HavingStarts(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => AddComponent(Component.Having, new BasicStringCondition { Operator = "starts", Column = column, Value = value, CaseSensitive = caseSensitive, EscapeCharacter = escapeCharacter, IsOr = GetOr(), IsNot = GetNot() });

    public Query HavingNotStarts(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Not().HavingStarts(column, value, caseSensitive, escapeCharacter);

    public Query OrHavingStarts(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().HavingStarts(column, value, caseSensitive, escapeCharacter);

    public Query OrHavingNotStarts(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().Not().HavingStarts(column, value, caseSensitive, escapeCharacter);

    public Query HavingEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => AddComponent(Component.Having, new BasicStringCondition { Operator = "ends", Column = column, Value = value, CaseSensitive = caseSensitive, EscapeCharacter = escapeCharacter, IsOr = GetOr(), IsNot = GetNot() });

    public Query HavingNotEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Not().HavingEnds(column, value, caseSensitive, escapeCharacter);

    public Query OrHavingEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().HavingEnds(column, value, caseSensitive, escapeCharacter);

    public Query OrHavingNotEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().Not().HavingEnds(column, value, caseSensitive, escapeCharacter);

    public Query HavingContains(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => AddComponent(Component.Having,
        new BasicStringCondition
        {
            Operator = "contains",
            Column = column,
            Value = value,
            CaseSensitive = caseSensitive,
            EscapeCharacter = escapeCharacter,
            IsOr = GetOr(),
            IsNot = GetNot(),
        });

    public Query HavingNotContains(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Not().HavingContains(column, value, caseSensitive, escapeCharacter);

    public Query OrHavingContains(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().HavingContains(column, value, caseSensitive, escapeCharacter);

    public Query OrHavingNotContains(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        => Or().Not().HavingContains(column, value, caseSensitive, escapeCharacter);

    public Query HavingBetween<T>(string column, T lower, T higher)
        where T : notnull
        => AddComponent(Component.Having, new BetweenCondition<T>
        {
            Column = column,
            IsOr = GetOr(),
            IsNot = GetNot(),
            Lower = lower,
            Higher = higher
        });

    public Query OrHavingBetween<T>(string column, T lower, T higher)
        where T : notnull
        => Or().HavingBetween(column, lower, higher);

    public Query HavingNotBetween<T>(string column, T lower, T higher)
        where T : notnull
        => Not().HavingBetween(column, lower, higher);

    public Query OrHavingNotBetween<T>(string column, T lower, T higher)
        where T : notnull
        => Or().Not().HavingBetween(column, lower, higher);

    public Query HavingIn<T>(string column, IEnumerable<T> values)
    {
        // If the developer has passed a string they most likely want a List<string>
        // since string is considered as List<char>
        if (values is string value)
        {
            return AddComponent(Component.Having, new InCondition<string>
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Values = new List<string> { value }
            });
        }

        return AddComponent(Component.Having, new InCondition<T>
        {
            Column = column,
            IsOr = GetOr(),
            IsNot = GetNot(),
            Values = values.Distinct().ToList()
        });
    }

    public Query OrHavingIn<T>(string column, IEnumerable<T> values)
        => Or().HavingIn(column, values);

    public Query HavingNotIn<T>(string column, IEnumerable<T> values)
        => Not().HavingIn(column, values);

    public Query OrHavingNotIn<T>(string column, IEnumerable<T> values)
        => Or().Not().HavingIn(column, values);

    public Query HavingIn(string column, Query query)
        => AddComponent(Component.Having,
        new InQueryCondition
        {
            Column = column,
            IsOr = GetOr(),
            IsNot = GetNot(),
            Query = query,
        });

    public Query HavingIn(string column, Func<Query, Query> callback)
    {
        var query = callback.Invoke(new Query());

        return HavingIn(column, query);
    }

    public Query OrHavingIn(string column, Query query)
        => Or().HavingIn(column, query);

    public Query OrHavingIn(string column, Func<Query, Query> callback)
        => Or().HavingIn(column, callback);

    public Query HavingNotIn(string column, Query query)
        => Not().HavingIn(column, query);

    public Query HavingNotIn(string column, Func<Query, Query> callback)
        => Not().HavingIn(column, callback);

    public Query OrHavingNotIn(string column, Query query)
        => Or().Not().HavingIn(column, query);

    public Query OrHavingNotIn(string column, Func<Query, Query> callback)
        => Or().Not().HavingIn(column, callback);

    /// <summary>
    /// Perform a sub query Having clause
    /// </summary>
    /// <param name="column"></param>
    /// <param name="op"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public Query Having(string column, string op, Func<Query, Query> callback)
    {
        var query = callback.Invoke(NewChild());

        return Having(column, op, query);
    }

    public Query Having(string column, string op, Query query)
        => AddComponent(Component.Having,
        new QueryCondition<Query>
        {
            Column = column,
            Operator = op,
            Query = query,
            IsNot = GetNot(),
            IsOr = GetOr()
        });

    public Query OrHaving(string column, string op, Query query)
        => Or().Having(column, op, query);

    public Query OrHaving(string column, string op, Func<Query, Query> callback)
        => Or().Having(column, op, callback);

    public Query HavingExists(Query query)
    {
        if (!query.HasComponent(Component.From))
        {
            throw new ArgumentException($"{nameof(FromClause)} cannot be empty if used inside a {nameof(HavingExists)} condition");
        }

        // simplify the query as much as possible
        query = query.Clone().ClearComponent(Component.Select)
            .SelectRaw("1")
            .Limit(1);

        return AddComponent(Component.Having, new ExistsCondition
        {
            Query = query,
            IsNot = GetNot(),
            IsOr = GetOr(),
        });
    }
    public Query HavingExists(Func<Query, Query> callback)
    {
        var childQuery = new Query().SetParent(this);
        return HavingExists(callback.Invoke(childQuery));
    }

    public Query HavingNotExists(Query query)
        => Not().HavingExists(query);

    public Query HavingNotExists(Func<Query, Query> callback)
    => Not().HavingExists(callback);

    public Query OrHavingExists(Query query)
        => Or().HavingExists(query);

    public Query OrHavingExists(Func<Query, Query> callback)
        => Or().HavingExists(callback);

    public Query OrHavingNotExists(Query query)
        => Or().Not().HavingExists(query);

    public Query OrHavingNotExists(Func<Query, Query> callback)
        => Or().Not().HavingExists(callback);

    public Query HavingDatePart(string part, string column, string op, object value)
        => AddComponent(Component.Having,
        new BasicDateCondition
        {
            Operator = op,
            Column = column,
            Value = value,
            Part = part,
            IsOr = GetOr(),
            IsNot = GetNot(),
        });

    public Query HavingNotDatePart(string part, string column, string op, object value)
        => Not().HavingDatePart(part, column, op, value);

    public Query OrHavingDatePart(string part, string column, string op, object value)
        => Or().HavingDatePart(part, column, op, value);

    public Query OrHavingNotDatePart(string part, string column, string op, object value)
        => Or().Not().HavingDatePart(part, column, op, value);

    public Query HavingDate(string column, string op, object value)
        => HavingDatePart("date", column, op, value);

    public Query HavingNotDate(string column, string op, object value)
        => Not().HavingDate(column, op, value);

    public Query OrHavingDate(string column, string op, object value)
        => Or().HavingDate(column, op, value);

    public Query OrHavingNotDate(string column, string op, object value)
        => Or().Not().HavingDate(column, op, value);

    public Query HavingTime(string column, string op, object value)
        => HavingDatePart("time", column, op, value);

    public Query HavingNotTime(string column, string op, object value)
        => Not().HavingTime(column, op, value);

    public Query OrHavingTime(string column, string op, object value)
        => Or().HavingTime(column, op, value);

    public Query OrHavingNotTime(string column, string op, object value)
        => Or().Not().HavingTime(column, op, value);

    public Query HavingDatePart(string part, string column, object value)
        => HavingDatePart(part, column, "=", value);

    public Query HavingNotDatePart(string part, string column, object value)
        => HavingNotDatePart(part, column, "=", value);

    public Query OrHavingDatePart(string part, string column, object value)
        => OrHavingDatePart(part, column, "=", value);

    public Query OrHavingNotDatePart(string part, string column, object value)
        => OrHavingNotDatePart(part, column, "=", value);

    public Query HavingDate(string column, object value)
        => HavingDate(column, "=", value);
    
    public Query HavingNotDate(string column, object value)
        => HavingNotDate(column, "=", value);

    public Query OrHavingDate(string column, object value)
        => OrHavingDate(column, "=", value);

    public Query OrHavingNotDate(string column, object value)
        => OrHavingNotDate(column, "=", value);

    public Query HavingTime(string column, object value)
        => HavingTime(column, "=", value);

    public Query HavingNotTime(string column, object value)
        => HavingNotTime(column, "=", value);

    public Query OrHavingTime(string column, object value)
        => OrHavingTime(column, "=", value);

    public Query OrHavingNotTime(string column, object value)
        => OrHavingNotTime(column, "=", value);
}
