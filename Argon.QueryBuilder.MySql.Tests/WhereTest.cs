using Argon.QueryBuilder.Tests;

namespace Argon.QueryBuilder.MySql.Tests;

public class WhereTest : WhereTestBase
{
    public override void WhereExists()
    {
        base.WhereExists();

        AssertSql("SELECT * FROM `users` WHERE EXISTS (SELECT 1 FROM `friends` WHERE `name` = @p0)");
    }

    public override void WhereBetween()
    {
        base.WhereBetween();

        AssertSql("SELECT * FROM `users` WHERE `created_at` BETWEEN @p0 AND @p1");
    }

    public override void WhereIn()
    {
        base.WhereIn();

        AssertSql("SELECT * FROM `users` WHERE `id` IN (@p0, @p1, @p2)");
    }

    public override void WhereNotIn()
    {
        base.WhereNotIn();

        AssertSql("SELECT * FROM `users` WHERE `id` NOT IN (@p0, @p1, @p2)");
    }

    public override void WhereInEmpty()
    {
        base.WhereInEmpty();

        AssertSql("SELECT * FROM `users` WHERE 1 = 0 /* IN [empty list] */");
    }

    public override void WhereNotInEmpty()
    {
        base.WhereNotInEmpty();

        AssertSql("SELECT * FROM `users` WHERE 1 = 1 /* NOT IN [empty list] */");
    }

    public override void WhereStarts()
    {
        base.WhereStarts();

        AssertSql("SELECT * FROM `users` WHERE `name` LIKE @p0");
    }

    public override void WhereEnds()
    {
        base.WhereEnds();

        AssertSql("SELECT * FROM `users` WHERE `name` LIKE @p0");
    }

    public override void WhereContains()
    {
        base.WhereContains();

        AssertSql("SELECT * FROM `users` WHERE `name` LIKE @p0");
    }

    public override void WhereTrue()
    {
        base.WhereTrue();

        AssertSql("SELECT * FROM `Table` WHERE `IsActive` = 1");
    }

    public override void WhereFalse()
    {
        base.WhereFalse();

        AssertSql("SELECT * FROM `Table` WHERE `IsActive` = 0");
    }

    public override void WhereWhenConditionTrue()
    {
        base.WhereWhenConditionTrue();

        AssertSql("SELECT * FROM `Table` WHERE `id` = @p0");
    }

    public override void WhereWhenConditionFalse()
    {
        base.WhereWhenConditionFalse();

        AssertSql("SELECT * FROM `Table`");
    }

    public override void WhereOr()
    {
        base.WhereOr();

        AssertSql("SELECT * FROM `Table` WHERE `id` = @p0 OR `name` = @p1");
    }

    public override void WhereAnd()
    {
        base.WhereAnd();

        AssertSql("SELECT * FROM `Table` WHERE `id` = @p0 AND `name` = @p1");
    }

    public override void WhereNestedOr()
    {
        base.WhereNestedOr();

        AssertSql("SELECT * FROM `Table` WHERE `id` = @p0 AND (`name` = @p1 OR `description` = @p2)");
    }

    public override void WhereQueryCondition()
    {
        base.WhereQueryCondition();

        AssertSql("SELECT * FROM `users` WHERE `id` = (SELECT `id` FROM `friends` WHERE `name` = @p0 LIMIT @p1)");
    }

    public override void WhereSubQuery()
    {
        base.WhereSubQuery();

        AssertSql("SELECT * FROM `users` WHERE (SELECT `id` FROM `friends` WHERE `name` = @p0 LIMIT @p1) = @p2");
    }

    public override void WhereInQuery()
    {
        base.WhereInQuery();

        AssertSql("SELECT * FROM `users` WHERE `id` IN (SELECT `id` FROM `friends` WHERE `name` = @p0 LIMIT @p1)");
    }
}
