using Argon.QueryBuilder.Clauses;

namespace Argon.QueryBuilder;

public partial class Query
{
    public Query Select(params string[] columns)
        => Select(columns.AsEnumerable());

    public Query Select(IEnumerable<string> columns)
    {
        Method = MethodType.Select;

        var cols = columns
            .Select(ExpandColumnExpression)
            .SelectMany(x => x)
            .ToArray();

        foreach (var (table, name, alias) in cols)
        {
            AddComponent(ComponentType.Select, new Column
            {
                Schema = table,
                Name = name,
                Alias = alias
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
        Method = MethodType.Select;

        AddComponent(ComponentType.Select, new RawColumn
        {
            Expression = sql,
            Bindings = bindings,
        });

        return this;
    }

    public Query Select(Query query, string alias)
    {
        Method = MethodType.Select;

        query = query.Clone();

        AddComponent(ComponentType.Select, new QueryColumn
        {
            Query = query.As(alias),
        });

        return this;
    }

    public Query Select(Func<Query, Query> callback, string alias)
        => Select(callback.Invoke(NewChild()), alias);

    public Query SelectAggregate(string aggregate, string column, Query? filter = null)
    {
        Method = MethodType.Select;

        AddComponent(ComponentType.Select, new AggregatedColumn
        {
            Column = new Column { Name = column },
            Aggregate = aggregate,
            Filter = filter,
        });

        return this;
    }

    private Query SelectAggregate(string aggregate, string column, Func<Query, Query>? filter)
        => SelectAggregate(aggregate, column, filter?.Invoke(NewChild()));

    public Query SelectSum(string column)
        => SelectAggregate("sum", column);

    public Query SelectCount(string column)
        => SelectAggregate("count", column);

    public Query SelectAvg(string column)
        => SelectAggregate("avg", column);

    public Query SelectMin(string column)
        => SelectAggregate("min", column);

    public Query SelectMax(string column)
        => SelectAggregate("max", column);
}
