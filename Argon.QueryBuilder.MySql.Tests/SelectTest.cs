using Argon.QueryBuilder.Tests;

namespace Argon.QueryBuilder.Benchmarks;

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

    public override void CascadedAndMultiReferencedCteAndBindings()
    {
        base.CascadedAndMultiReferencedCteAndBindings();
    }

    public override void CascadedCteAndBindings()
    {
        base.CascadedCteAndBindings();
    }

    public override void CombineRawWithPlaceholders()
    {
        base.CombineRawWithPlaceholders();
    }

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

    public override void MultipleCtesAndBindings()
    {
        base.MultipleCtesAndBindings();
    }

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
}
