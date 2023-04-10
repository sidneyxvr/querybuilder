using Argon.QueryBuilder.Tests;

namespace Argon.QueryBuilder.MySql.Tests;

public class OrderByTest : OrderByTestBase
{
    public override void OrderBySingleField()
    {
        base.OrderBySingleField();

        AssertSql("SELECT * FROM `blogs` ORDER BY `name`");
    }

    public override void OrderByMultipleFields()
    {
        base.OrderByMultipleFields();

        AssertSql("SELECT * FROM `blogs` ORDER BY `name`, `title`, `authorName`");
    }
}
