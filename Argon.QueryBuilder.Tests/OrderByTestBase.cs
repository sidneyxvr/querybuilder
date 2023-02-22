using Xunit;

namespace Argon.QueryBuilder.Tests;

public class OrderByTestBase : TestBase
{
    [Fact]
    public virtual void OrderBySingleField()
        => AssertQuery(new Query()
        .From("blogs")
        .OrderBy("name"));

    [Fact]
    public virtual void OrderByMultipleFields()
        => AssertQuery(new Query()
        .From("blogs")
        .OrderBy("name", "title", "authorName"));
}
