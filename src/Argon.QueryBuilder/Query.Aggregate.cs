using Argon.QueryBuilder.Clauses;

namespace Argon.QueryBuilder;

public partial class Query
{
    public Query AsAggregate(string type, string[]? columns = null)
    {
        Method = MethodType.Aggregate;

        AggregateColumns.Clear();

        AddComponent(ComponentType.Aggregate, new AggregateClause
        {
            Type = type,
            Columns = columns?.Select(c =>
            {
                var (name, alias) = ExpandColumn(c);

                return new Column
                {
                    Name = name,
                    Alias = alias,
                };
            })
            .ToList() ?? new List<Column>(),
        });

        return this;
    }

    public Query AsCount(string[]? columns = null)
    {
        var cols = columns?.ToList() ?? new List<string> { };

        if (cols.Count == 0)
        {
            cols.Add("*");
        }

        return AsAggregate("count", cols.ToArray());
    }

    public Query AsAvg(string column)
        => AsAggregate("avg", new string[] { column });

    public Query AsAverage(string column)
        => AsAvg(column);

    public Query AsSum(string column)
        => AsAggregate("sum", new[] { column });

    public Query AsMax(string column)
        => AsAggregate("max", new[] { column });

    public Query AsMin(string column)
        => AsAggregate("min", new[] { column });
}
