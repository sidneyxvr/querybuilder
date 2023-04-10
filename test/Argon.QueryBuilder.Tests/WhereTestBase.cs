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

    [Fact]
    public virtual void WhereLike()
        => AssertQuery(new Query("users")
            .WhereLike("name", "test"));

    [Fact]
    public virtual void WhereDumbNull()
    => AssertQuery(new Query("users")
        .Where("name", "=", (object?)null));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void WhereDumbBoolean(bool value)
        => AssertQuery(new Query("users")
            .Where("isActive", "=", value));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void WhereDumbBooleanNotEqual(bool value)
     => AssertQuery(new Query("users")
         .Where("isActive", "!=", value));

    [Fact]
    public virtual void WhereNot()
        => AssertQuery(new Query("users")
            .WhereNot("id", 1));

    [Fact]
    public virtual void WhereOrNot()
        => AssertQuery(new Query("users")
        .Where("name", "test").OrWhereNot("id", 1));

    [Fact]
    public virtual void WhereConstants()
        => AssertQuery(new Query("users")
        .Where(new Test { Id = 1, Name = "test" }));

    [Fact]
    public virtual void WhereNotCallback()
        => AssertQuery(new Query("users")
        .WhereNot(q => q.Where("id", 1).OrWhere("name", "test")));

    [Fact]
    public virtual void WhereOrCalback()
    => AssertQuery(new Query("users")
    .Where(q => q.Where("id", 1)
        .OrWhere(q => q.Where("name", "test")
            .OrWhere("name", "value"))));

    [Fact]
    public virtual void WhereOrNotCalback()
        => AssertQuery(new Query("users")
        .Where(q => q.Where("id", 1)
            .OrWhereNot(q => q.Where("name", "test")
                .OrWhere("name", "value"))));

    [Fact]
    public virtual void WhereOrNull()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereNull("name"));

    [Fact]
    public virtual void WhereOrNotNull()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereNotNull("name"));

    [Fact]
    public virtual void WhereOrTrue()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereTrue("isActive"));

    [Fact]
    public virtual void WhereOrFalse()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereFalse("isActive"));

    [Fact]
    public virtual void WhereNotLike()
        => AssertQuery(new Query("users")
        .WhereNotLike("name", "test"));

    [Fact]
    public virtual void WhereOrLike()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereLike("name", "test"));

    [Fact]
    public virtual void WhereOrNotLike()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereNotLike("name", "test"));

    [Fact]
    public virtual void WhereNotStarts()
        => AssertQuery(new Query("users")
        .WhereNotStarts("name", "test"));

    [Fact]
    public virtual void WhereOrStarts()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereStarts("name", "test"));

    [Fact]
    public virtual void WhereOrNotStarts()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereNotStarts("name", "test"));

    [Fact]
    public virtual void WhereNotEnds()
        => AssertQuery(new Query("users")
        .WhereNotEnds("name", "test"));

    [Fact]
    public virtual void WhereOrEnds()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereEnds("name", "test"));

    [Fact]
    public virtual void WhereOrNotEnds()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereNotEnds("name", "test"));


    [Fact]
    public virtual void WhereNotContains()
        => AssertQuery(new Query("users")
        .WhereNotContains("name", "test"));

    [Fact]
    public virtual void WhereOrContains()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereContains("name", "test"));

    [Fact]
    public virtual void WhereOrNotContains()
        => AssertQuery(new Query("users")
        .Where("id", 1).OrWhereNotContains("name", "test"));

    [Fact]
    public virtual void WhereNotBetween()
=> AssertQuery(new Query("users")
.WhereNotBetween("createdAt", DateTime.UtcNow.AddYears(-1), DateTime.UtcNow));

    [Fact]
    public virtual void WhereOrBetween()
=> AssertQuery(new Query("users")
.Where("id", 1).OrWhereBetween("createdAt", DateTime.UtcNow.AddYears(-1), DateTime.UtcNow));

    [Fact]
    public virtual void WhereOrNotBetween()
=> AssertQuery(new Query("users")
.Where("id", 1).OrWhereNotBetween("createdAt", DateTime.UtcNow.AddYears(-1), DateTime.UtcNow));

    [Fact]
    public virtual void WhereOrIn()
        => AssertQuery(new Query("users")
            .Where("name", "test").OrWhereIn("id", new[] { 1, 2 }));

    [Fact]
    public virtual void WhereOrNotIn()
         => AssertQuery(new Query("users")
             .Where("name", "test").OrWhereNotIn("id", new[] { 1, 2 }));

    [Fact]
    public virtual void WhereInCallback()
         => AssertQuery(new Query("users")
             .WhereIn("id", q => new Query("posts").Where("id", 1).Select("userId")));

    [Fact]
    public virtual void WhereOrInQuery()
         => AssertQuery(new Query("users")
             .Where("name", "test").OrWhereIn("id", new Query("posts").Where("id", 1).Select("userId")));

    [Fact]
    public virtual void WhereOrInCallback()
         => AssertQuery(new Query("users")
             .Where("name", "test").OrWhereIn("id", q => new Query("posts").Where("id", 1).Select("userId")));

    [Fact]
    public virtual void WhereNotInQuery()
        => AssertQuery(new Query("users")
            .WhereNotIn("id", new Query("posts").Where("id", 1).Select("userId")));

    [Fact]
    public virtual void WhereNotInCallback()
        => AssertQuery(new Query("users")
            .WhereNotIn("id", q => new Query("posts").Where("id", 1).Select("userId")));

    [Fact]
    public virtual void WhereOrNotInQuery()
        => AssertQuery(new Query("users")
            .Where("name", "test").OrWhereNotIn("id", new Query("posts").Where("id", 1).Select("userId")));

    [Fact]
    public virtual void WhereOrNotInCallback()
        => AssertQuery(new Query("users")
            .Where("name", "test").OrWhereNotIn("id", q => new Query("posts").Where("id", 1).Select("userId")));

    [Fact]
    public virtual void WhereQuery()
        => AssertQuery(new Query("users")
            .Where("id", "=", new Query("posts").Where("id", 1).Select("userId")));

    [Fact]
    public virtual void WhereCallback()
        => AssertQuery(new Query("users")
            .Where("id", "=", q => new Query("posts").Where("id", 1).Select("userId")));
}

public class Test
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
