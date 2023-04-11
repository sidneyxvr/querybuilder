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

    public override void WhereLike()
    {
        base.WhereLike();

        AssertSql("SELECT * FROM `users` WHERE `name` LIKE @p0");
    }

    public override void WhereDumbNull()
    {
        base.WhereDumbNull();

        AssertSql("SELECT * FROM `users` WHERE `name` IS NULL");
    }

    public override void WhereDumbBoolean(bool value)
    {
        base.WhereDumbBoolean(value);

        AssertSql($"SELECT * FROM `users` WHERE `isActive` = {(value ? "1": "0")}");
    }

    public override void WhereDumbBooleanNotEqual(bool value)
    {
        base.WhereDumbBooleanNotEqual(value);

        AssertSql($"SELECT * FROM `users` WHERE `isActive` != {(value ? "1": "0")}");
    }

    public override void WhereNot()
    {
        base.WhereNot();

        AssertSql("SELECT * FROM `users` WHERE NOT `id` = @p0");
    }

    public override void WhereOrNot()
    {
        base.WhereOrNot();

        AssertSql("SELECT * FROM `users` WHERE `name` = @p0 OR NOT `id` = @p1");
    }

    public override void WhereConstants()
    {
        base.WhereConstants();

        AssertSql("SELECT * FROM `users` WHERE `Id` = @p0 AND `Name` = @p1");
    }

    public override void WhereNotCallback()
    {
        base.WhereNotCallback();

        AssertSql("SELECT * FROM `users` WHERE NOT (`id` = @p0 OR `name` = @p1)");
    }

    public override void WhereOrCalback()
    {
        base.WhereOrCalback();

        AssertSql("SELECT * FROM `users` WHERE (`id` = @p0 OR (`name` = @p1 OR `name` = @p2))");
    }

    public override void WhereOrNotCalback()
    {
        base.WhereOrNotCalback();

        AssertSql("SELECT * FROM `users` WHERE (`id` = @p0 OR NOT (`name` = @p1 OR `name` = @p2))");
    }

    public override void WhereOrNull()
    {
        base.WhereOrNull();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR `name` IS NULL");
    }

    public override void WhereOrNotNull()
    {
        base.WhereOrNotNull();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR `name` IS NOT NULL");
    }

    public override void WhereOrTrue()
    {
        base.WhereOrTrue();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR `isActive` = 1");
    }

    public override void WhereOrFalse()
    {
        base.WhereOrFalse();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR `isActive` = 0");
    }

    public override void WhereNotLike()
    {
        base.WhereNotLike();

        AssertSql("SELECT * FROM `users` WHERE NOT (`name` LIKE @p0)");
    }

    public override void WhereOrLike()
    {
        base.WhereOrLike();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR `name` LIKE @p1");
    }

    public override void WhereOrNotLike()
    {
        base.WhereOrNotLike();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR NOT (`name` LIKE @p1)");
    }

    public override void WhereNotStarts()
    {
        base.WhereNotStarts();

        AssertSql("SELECT * FROM `users` WHERE NOT (`name` LIKE @p0)",
            new (string, object)[] {("@p0", "test%")});
    }

    public override void WhereOrStarts()
    {
        base.WhereOrStarts();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR `name` LIKE @p1",
            new (string, object)[] { ("@p0", 1), ("@p1", "test%") });
    }

    public override void WhereOrNotStarts()
    {
        base.WhereOrNotStarts();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR NOT (`name` LIKE @p1)",
            new (string, object)[] { ("@p0", 1), ("@p1", "test%") });
    }

    public override void WhereNotEnds()
    {
        base.WhereNotEnds();

        AssertSql("SELECT * FROM `users` WHERE NOT (`name` LIKE @p0)",
            new (string, object)[] { ("@p0", "%test") });
    }

    public override void WhereOrEnds()
    {
        base.WhereOrEnds();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR `name` LIKE @p1",
            new (string, object)[] { ("@p0", 1), ("@p1", "%test") });
    }

    public override void WhereOrNotEnds()
    {
        base.WhereOrNotEnds();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR NOT (`name` LIKE @p1)",
            new (string, object)[] { ("@p0", 1), ("@p1", "%test") });
    }

    public override void WhereNotContains()
    {
        base.WhereNotContains();

        AssertSql("SELECT * FROM `users` WHERE NOT (`name` LIKE @p0)",
            new (string, object)[] { ("@p0", "%test%") });
    }

    public override void WhereOrContains()
    {
        base.WhereOrContains();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR `name` LIKE @p1",
            new (string, object)[] { ("@p0", 1), ("@p1", "%test%") });
    }

    public override void WhereOrNotContains()
    {
        base.WhereOrNotContains();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR NOT (`name` LIKE @p1)",
            new (string, object)[] { ("@p0", 1), ("@p1", "%test%") });
    }

    public override void WhereNotBetween()
    {
        base.WhereNotBetween();

        AssertSql("SELECT * FROM `users` WHERE `createdAt` NOT BETWEEN @p0 AND @p1");
    }

    public override void WhereOrBetween()
    {
        base.WhereOrBetween();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR `createdAt` BETWEEN @p1 AND @p2");
    }

    public override void WhereOrNotBetween()
    {
        base.WhereOrNotBetween();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR `createdAt` NOT BETWEEN @p1 AND @p2");
    }

    public override void WhereOrIn()
    {
        base.WhereOrIn();

        AssertSql("SELECT * FROM `users` WHERE `name` = @p0 OR `id` IN (@p1, @p2)");
    }

    public override void WhereOrNotIn()
    {
        base.WhereOrNotIn();

        AssertSql("SELECT * FROM `users` WHERE `name` = @p0 OR `id` NOT IN (@p1, @p2)");
    }

    public override void WhereInCallback()
    {
        base.WhereInCallback();

        AssertSql("SELECT * FROM `users` WHERE `id` IN (SELECT `userId` FROM `posts` WHERE `id` = @p0)");
    }

    public override void WhereOrInQuery()
    {
        base.WhereOrInQuery();

        AssertSql("SELECT * FROM `users` WHERE `name` = @p0 OR `id` IN (SELECT `userId` FROM `posts` WHERE `id` = @p1)");
    }

    public override void WhereOrInCallback()
    {
        base.WhereOrInCallback();

        AssertSql("SELECT * FROM `users` WHERE `name` = @p0 OR `id` IN (SELECT `userId` FROM `posts` WHERE `id` = @p1)");
    }

    public override void WhereNotInQuery()
    {
        base.WhereNotInQuery();

        AssertSql("SELECT * FROM `users` WHERE `id` NOT IN (SELECT `userId` FROM `posts` WHERE `id` = @p0)");
    }

    public override void WhereNotInCallback()
    {
        base.WhereNotInCallback();

        AssertSql("SELECT * FROM `users` WHERE `id` NOT IN (SELECT `userId` FROM `posts` WHERE `id` = @p0)");
    }

    public override void WhereOrNotInQuery()
    {
        base.WhereOrNotInQuery();

        AssertSql("SELECT * FROM `users` WHERE `name` = @p0 OR `id` NOT IN (SELECT `userId` FROM `posts` WHERE `id` = @p1)");
    }

    public override void WhereOrNotInCallback()
    {
        base.WhereOrNotInCallback();

        AssertSql("SELECT * FROM `users` WHERE `name` = @p0 OR `id` NOT IN (SELECT `userId` FROM `posts` WHERE `id` = @p1)");
    }

    public override void WhereQuery()
    {
        base.WhereQuery();

        AssertSql("SELECT * FROM `users` WHERE `id` = (SELECT `userId` FROM `posts` WHERE `id` = @p0)");
    }

    public override void WhereCallback()
    {
        base.WhereCallback();

        AssertSql("SELECT * FROM `users` WHERE `id` = (SELECT `userId` FROM `posts` WHERE `id` = @p0)");
    }

    public override void WhereInString()
    {
        base.WhereInString();

        AssertSql("SELECT * FROM `users` WHERE `name` IN (@p0)",
            new (string, object)[] { ("@p0", "test") } );
    }

    public override void WhereOrSub()
    {
        base.WhereOrSub();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR (SELECT `id` FROM `posts` WHERE `id` = @p1) = @p2");
    }

    public override void WhereOrSubOperation()
    {
        base.WhereOrSubOperation();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR (SELECT `id` FROM `posts` WHERE `id` = @p1) != @p2");
    }

    public override void WhereOrColumnSub()
    {
        base.WhereOrColumnSub();

        AssertSql("SELECT * FROM `users` WHERE `name` = @p0 OR `id` = (SELECT `userId` FROM `posts` WHERE `id` = @p1)");
    }

    public override void WhereOrColumnCallback()
    {
        base.WhereOrColumnCallback();

        AssertSql("SELECT * FROM `users` WHERE `name` = @p0 OR `id` = (SELECT `userId` FROM `posts` WHERE `id` = @p1)");
    }

    public override void WhereNotExistsQuery()
    {
        base.WhereNotExistsQuery();

        AssertSql("SELECT * FROM `users` AS `u` WHERE NOT EXISTS (SELECT 1 FROM `posts` AS `p` WHERE `u`.`id` = `p`.`userId`)");
    }

    public override void WhereNotExistsCallback()
    {
        base.WhereNotExistsCallback();

        AssertSql("SELECT * FROM `users` AS `u` WHERE NOT EXISTS (SELECT 1 FROM `posts` AS `p` WHERE `u`.`id` = `p`.`userId`)");
    }

    public override void WhereOrExistsQuery()
    {
        base.WhereOrExistsQuery();

        AssertSql("SELECT * FROM `users` AS `u` WHERE `name` = @p0 OR EXISTS (SELECT 1 FROM `posts` AS `p` WHERE `u`.`id` = `p`.`userId`)");
    }

    public override void WhereOrExistsCallback()
    {
        base.WhereOrExistsCallback();

        AssertSql("SELECT * FROM `users` AS `u` WHERE `name` = @p0 OR EXISTS (SELECT 1 FROM `posts` AS `p` WHERE `u`.`id` = `p`.`userId`)");
    }

    public override void WhereOrNotExistsQuery()
    {
        base.WhereOrNotExistsQuery();
        AssertSql("SELECT * FROM `users` AS `u` WHERE `name` = @p0 OR NOT EXISTS (SELECT 1 FROM `posts` AS `p` WHERE `u`.`id` = `p`.`userId`)");

    }

    public override void WhereOrNotExistsCallback()
    {
        base.WhereOrNotExistsCallback();

        AssertSql("SELECT * FROM `users` AS `u` WHERE `name` = @p0 OR NOT EXISTS (SELECT 1 FROM `posts` AS `p` WHERE `u`.`id` = `p`.`userId`)");
    }

    public override void WhereDatePart()
    {
        base.WhereDatePart();

        AssertSql("SELECT * FROM `users` WHERE YEAR(`createdAt`) = @p0");
    }

    public override void WhereDateNotPart()
    {
        base.WhereDateNotPart();

        AssertSql("SELECT * FROM `users` WHERE NOT (YEAR(`createdAt`) = @p0)");
    }

    public override void WhereOrDatePart()
    {
        base.WhereOrDatePart();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR YEAR(`createdAt`) = @p1");
    }

    public override void WhereOrNotDatePart()
    {
        base.WhereOrNotDatePart();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR NOT (YEAR(`createdAt`) = @p1)");
    }

    public override void WhereDate()
    {
        base.WhereDate();

        AssertSql("SELECT * FROM `users` WHERE DATE(`createdAt`) = @p0");
    }

    public override void WhereNotDate()
    {
        base.WhereNotDate();

        AssertSql("SELECT * FROM `users` WHERE NOT (DATE(`createdAt`) = @p0)");
    }

    public override void WhereOrDate()
    {
        base.WhereOrDate();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR DATE(`createdAt`) = @p1");
    }

    public override void WhereOrNotDate()
    {
        base.WhereOrNotDate();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR NOT (DATE(`createdAt`) = @p1)");
    }

    public override void WhereTime()
    {
        base.WhereTime();

        AssertSql("SELECT * FROM `users` WHERE TIME(`createdAt`) = @p0");
    }

    public override void WhereNotTime()
    {
        base.WhereNotTime();

        AssertSql("SELECT * FROM `users` WHERE NOT (TIME(`createdAt`) = @p0)");
    }

    public override void WhereOrTime()
    {
        base.WhereOrTime();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR TIME(`createdAt`) = @p1");
    }

    public override void WhereOrNotTime()
    {
        base.WhereOrNotTime();

        AssertSql("SELECT * FROM `users` WHERE `id` = @p0 OR NOT (TIME(`createdAt`) = @p1)");
    }
}
