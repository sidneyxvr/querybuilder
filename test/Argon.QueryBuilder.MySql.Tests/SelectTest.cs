using Argon.QueryBuilder.Tests;

namespace Argon.QueryBuilder.MySql.Tests;

public class SelectTest : SelectTestBase
{
    public override void BasicJoin()
    {
        base.BasicJoin();

        AssertSql("SELECT * FROM `users` INNER JOIN `countries` ON `countries`.`id` = `users`.`country_id`");
    }

    public override void BasicSelect()
    {
        base.BasicSelect();

        AssertSql("SELECT `id`, `name` FROM `users`");
    }

    public override void BasicSelectEnumerable()
    {
        base.BasicSelectEnumerable();

        AssertSql("SELECT `id`, `name` FROM `users`");
    }

    public override void BasicSelectWhereBindingIsEmptyOrNull()
    {
        base.BasicSelectWhereBindingIsEmptyOrNull();

        AssertSql("SELECT `id`, `name` FROM `users` WHERE `author` = @p0 OR `author` IS NULL");
    }

    public override void BasicSelectWithAlias()
    {
        base.BasicSelectWithAlias();

        AssertSql("SELECT `id`, `name` FROM `users` AS `u`");
    }

    //public override void CascadedAndMultiReferencedCteAndBindings()
    //{
    //    base.CascadedAndMultiReferencedCteAndBindings();
    //}

    //public override void CascadedCteAndBindings()
    //{
    //    base.CascadedCteAndBindings();
    //}

    public override void ExpandedSelect()
    {
        base.ExpandedSelect();

        AssertSql("SELECT `users`.`id`, `users`.`name`, `users`.`age` FROM `users`");
    }

    public override void JoinTypes(string given, string output)
    {
        base.JoinTypes(given, output);

        AssertSql($"SELECT * FROM `users` {output} `countries` ON `countries`.`id` = `users`.`country_id`");
    }

    public override void Limit()
    {
        base.Limit();

        AssertSql("SELECT `id`, `name` FROM `users` LIMIT @p0");
    }

    public override void LimitOffset()
    {
        base.LimitOffset();

        AssertSql("SELECT * FROM `users` LIMIT @p0 OFFSET @p1");
    }

    //public override void MultipleCtesAndBindings()
    //{
    //    base.MultipleCtesAndBindings();
    //}

    public override void Offset()
    {
        base.Offset();

        AssertSql("SELECT * FROM `users` LIMIT 18446744073709551615 OFFSET @p0");
    }

    public override void UnionWithBindings()
    {
        base.UnionWithBindings();

        AssertSql("SELECT * FROM `Phones` UNION SELECT * FROM `Laptops` WHERE `Type` = @p0");
    }

    public override void SelectSum()
    {
        base.SelectSum();

        AssertSql("SELECT SUM(`value`) FROM `users`");
    }

    public override void SelectSumWithAlias()
    {
        base.SelectSumWithAlias();

        AssertSql("SELECT SUM(`value`) AS Total FROM `users`");
    }

    public override void SelectWithAlias()
    {
        base.SelectWithAlias();

        AssertSql("SELECT `id` AS Id, `name` AS Name FROM `users`");
    }

    public override void SelectQuery()
    {
        base.SelectQuery();

        AssertSql("SELECT (SELECT `p`.`createdAt` FROM `posts` AS `p` WHERE `p`.`userId` = `u`.`id`) AS `lastPublishDate` FROM `users` AS `u`");
    }

    public override void SelectQueryLambda()
    {
        base.SelectQueryLambda();

        AssertSql("SELECT (SELECT `p`.`createdAt` FROM `posts` AS `p` WHERE `p`.`userId` = `u`.`id`) AS `lastPublishDate` FROM `users` AS `u`");
    }

    public override void SelectParams()
    {
        base.SelectParams();

        AssertSql("SELECT `id`, `name` FROM `users`");
    }

    public override void SelectEnumerable()
    {
        base.SelectEnumerable();

        AssertSql("SELECT `id`, `name` FROM `users`");
    }

    public override void SelectConst()
    {
        base.SelectConst();

        AssertSql("SELECT 1 FROM `users`");
    }

    public override void SelectCount()
    {
        base.SelectCount();

        AssertSql("SELECT COUNT(`id`) FROM `users`");
    }

    public override void SelectAvg()
    {
        base.SelectAvg();

        AssertSql("SELECT AVG(`id`) FROM `users`");
    }

    public override void SelectMin()
    {
        base.SelectMin();

        AssertSql("SELECT MIN(`id`) FROM `users`");
    }

    public override void SelectMax()
    {
        base.SelectMax();

        AssertSql("SELECT MAX(`id`) FROM `users`");
    }
}
