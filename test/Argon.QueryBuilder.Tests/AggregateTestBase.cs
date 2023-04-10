using Xunit;

namespace Argon.QueryBuilder.Tests;

public class AggregateTestBase : TestBase
{
    [Fact]
    public virtual void Count()
        => AssertQuery(new Query("A").AsCount());

    [Fact]
    public virtual void CountMultipleColumns()
        => AssertQuery(new Query("A").AsCount(new[] { "ColumnA", "ColumnB" }));

    [Fact]
    public virtual void DistinctCount()
        => AssertQuery(new Query("A").Distinct().AsCount());

    [Fact]
    public virtual void DistinctCountMultipleColumns()
        => AssertQuery(new Query("A").Distinct().AsCount(new[] { "ColumnA", "ColumnB" }));

    [Fact]
    public virtual void Average()
        => AssertQuery(new Query("A").AsAverage("TTL"));

    [Fact]
    public virtual void Sum()
        => AssertQuery(new Query("A").AsSum("PacketsDropped"));

    [Fact]
    public virtual void Max()
        => AssertQuery(new Query("A").AsMax("LatencyMs"));

    [Fact]
    public virtual void Min()
        => AssertQuery(new Query("A").AsMin("LatencyMs"));
}
