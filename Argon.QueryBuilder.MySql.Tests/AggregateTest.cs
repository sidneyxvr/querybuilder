using Argon.QueryBuilder.Tests;

namespace Argon.QueryBuilder.MySql.Tests;

public class AggregateTest : AggregateTestBase
{
    public override void Average()
    {
        base.Average();

        AssertSql("SELECT AVG(`TTL`) AS `avg` FROM `A`");
    }

    public override void Count()
    {
        base.Count();

        AssertSql("SELECT COUNT(*) AS `count` FROM `A`");
    }

    public override void CountMultipleColumns()
    {
        base.CountMultipleColumns();

        AssertSql("SELECT COUNT(*) AS `count` FROM (SELECT 1 FROM `A` WHERE `ColumnA` IS NOT NULL AND `ColumnB` IS NOT NULL) AS `countQuery`");
    }

    public override void DistinctCount()
    {
        base.DistinctCount();

        AssertSql("SELECT COUNT(*) AS `count` FROM (SELECT DISTINCT * FROM `A`) AS `countQuery`");
    }

    public override void DistinctCountMultipleColumns()
    {
        base.DistinctCountMultipleColumns();

        AssertSql("SELECT COUNT(*) AS `count` FROM (SELECT DISTINCT `ColumnA`, `ColumnB` FROM `A`) AS `countQuery`");
    }

    public override void Max()
    {
        base.Max();

        AssertSql("SELECT MAX(`LatencyMs`) AS `max` FROM `A`");
    }

    public override void Min()
    {
        base.Min();

        AssertSql("SELECT MIN(`LatencyMs`) AS `min` FROM `A`");
    }

    public override void Sum()
    {
        base.Sum();

        AssertSql("SELECT SUM(`PacketsDropped`) AS `sum` FROM `A`");
    }
}
