namespace SqlKata;

public partial class Query
{
    public Query AsAggregate(string type, string[]? columns = null)
    {
        Method = "aggregate";

        ClearComponent("aggregate")
            .AddComponent("aggregate", new AggregateClause
            {
                Type = type,
                Columns = columns?.ToList() ?? new List<string>(),
            });

        return this;
    }

    public Query AsCount(string[]? columns = null)
    {
        var cols = columns?.ToList() ?? new List<string> { };

        if (!cols.Any())
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
