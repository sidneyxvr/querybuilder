using Argon.QueryBuilder.Clauses;
using System.Text.RegularExpressions;

namespace Argon.QueryBuilder;

public partial class Query
{
    [GeneratedRegex(@"^(?:\w+\.){1,2}{(.*)}")]
    private static partial Regex ExpandRegex();


    [GeneratedRegex("\\s*,\\s*")]
    private static partial Regex ColumnRegex();

    public Query Select(params string[] columns)
        => Select(columns.AsEnumerable());

    public Query Select(IEnumerable<string> columns)
    {
        Method = "select";

        columns = columns
            .Select(ExpandExpression)
            .SelectMany(x => x)
            .ToArray();

        foreach (var column in columns)
        {
            AddComponent(Component.Select, new Column
            {
                Name = column
            });
        }

        return this;
    }

    /// <summary>
    /// Add a new "raw" select expression to the query.
    /// </summary>
    /// <returns></returns>
    public Query SelectRaw(string sql, params object[] bindings)
    {
        Method = "select";

        AddComponent(Component.Select, new RawColumn
        {
            Expression = sql,
            Bindings = bindings,
        });

        return this;
    }

    public Query Select(Query query, string alias)
    {
        Method = "select";

        query = query.Clone();

        AddComponent(Component.Select, new QueryColumn
        {
            Query = query.As(alias),
        });

        return this;
    }

    public Query Select(Func<Query, Query> callback, string alias)
        => Select(callback.Invoke(NewChild()), alias);

    public Query SelectAggregate(string aggregate, string column, Query? filter = null)
    {
        Method = "select";

        AddComponent(Component.Select, new AggregatedColumn
        {
            Column = new Column { Name = column },
            Aggregate = aggregate,
            Filter = filter,
        });

        return this;
    }

    public Query SelectAggregate(string aggregate, string column, Func<Query, Query>? filter)
        => filter is null
        ? SelectAggregate(aggregate, column)
        : SelectAggregate(aggregate, column, filter.Invoke(NewChild()));

    public Query SelectSum(string column, Func<Query, Query>? filter = null)
        => SelectAggregate("sum", column, filter);

    public Query SelectCount(string column, Func<Query, Query>? filter = null)
        => SelectAggregate("count", column, filter);

    public Query SelectAvg(string column, Func<Query, Query>? filter = null)
        => SelectAggregate("avg", column, filter);

    public Query SelectMin(string column, Func<Query, Query>? filter = null)
        => SelectAggregate("min", column, filter);

    public Query SelectMax(string column, Func<Query, Query>? filter = null)
        => SelectAggregate("max", column, filter);

    private static List<string> ExpandExpression(string expression)
    {
        var match = ExpandRegex().Match(expression);

        if (!match.Success)
        {
            // we did not found a match return the string as is.
            return new List<string>(1) { expression };
        }

        var table = expression[..expression.IndexOf(".{")];

        var captures = match.Groups[1].Value;

        var cols = ColumnRegex()
            .Split(captures)
            .Select(x => $"{table}.{x.Trim()}")
            .ToList();

        return cols;
    }
}
