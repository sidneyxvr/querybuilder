using Xunit;

namespace Argon.QueryBuilder.Tests;

public class WhereTestBase : TestBase
{
    [Fact]
    public virtual void WhereExists()
        => AssertQuery(new Query()
            .From("users")
            .WhereExists(q => q.Select("1").From("friends").Where("name", "test")));

    [Fact]
    public virtual void WhereBetween()
        => AssertQuery(new Query()
            .From("users")
            .WhereBetween("created_at", DateTime.Now.AddDays(-30), DateTime.Now));

    [Fact]
    public virtual void WhereIn()
        => AssertQuery(new Query()
            .From("users")
            .WhereIn("id", new[] { 1, 2, 3 }));

    [Fact]
    public virtual void WhereNotIn()
        => AssertQuery(new Query()
            .From("users")
            .WhereNotIn("id", new[] { 1, 2, 3 }));

    [Fact]
    public virtual void WhereInEmpty()
        => AssertQuery(new Query()
            .From("users")
            .WhereIn("id", Array.Empty<int>()));

    [Fact]
    public virtual void WhereNotInEmpty()
        => AssertQuery(new Query()
            .From("users")
            .WhereNotIn("id", Array.Empty<int>()));

    [Fact]
    public virtual void WhereStarts()
        => AssertQuery(new Query()
            .From("users")
            .WhereStarts("name", "test"));

    [Fact]
    public virtual void WhereEnds()
        => AssertQuery(new Query()
            .From("users")
            .WhereEnds("name", "test"));

    [Fact]
    public virtual void WhereContains()
        => AssertQuery(new Query()
            .From("users")
            .WhereContains("name", "test"));

    [Fact]
    public virtual void WhereTrue()
        => AssertQuery(new Query("Table")
            .WhereTrue("IsActive"));

    [Fact]
    public virtual void WhereFalse()
        => AssertQuery(new Query("Table")
            .WhereFalse("IsActive"));

    [Fact]
    public virtual void WhereWhenConditionTrue()
        => AssertQuery(new Query("Table")
            .When(true, q => q.Where("id", 1)));

    [Fact]
    public virtual void WhereWhenConditionFalse()
        => AssertQuery(new Query("Table")
            .When(false, q => q.Where("id", 1)));


    [Fact]
    public virtual void WhereOr()
        => AssertQuery(new Query("Table")
            .Where("id", 1).OrWhere("name", "test"));

    [Fact]
    public virtual void WhereAnd()
        => AssertQuery(new Query("Table")
            .Where("id", 1).Where("name", "test"));

    [Fact]
    public virtual void WhereNestedOr()
        => AssertQuery(new Query("Table")
        .Where("id", 1)
        .Where(q
            => q.Where("name", "test")
            .OrWhere("description", "value")));


    //[Fact]
    //public virtual void WhereQueryCondition()
    //    => AssertQuery(new Query("users")
    //    .Where("id", "=", q
    //        => q.From("friends")
    //        .Select("id")
    //        .Where("name", "test")
    //        .Limit(1)));

    [Fact]
    public virtual void WhereQueryCondition()
        => AssertQuery(new Query("users")
        .Where("id", "=",
            new Query().From("friends")
                .Select("id")
                .Where("name", "test")
                .Limit(1)));


    [Fact]
    public virtual void WhereSubQuery()
    => AssertQuery(new Query("users")
    .WhereSub(new Query().From("friends")
        .Select("id")
        .Where("name", "test")
        .Limit(1), "1"));


    [Fact]
    public virtual void WhereInQuery()
    => AssertQuery(new Query("users")
    .WhereIn("id", new Query().From("friends")
        .Select("id")
        .Where("name", "test")
        .Limit(1)));
}
