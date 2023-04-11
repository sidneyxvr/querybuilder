using Argon.QueryBuilder.Tests;

namespace Argon.QueryBuilder.MySql.Tests;

public class QueryCombineTestBase : TestBase
{
    [Fact]
    public virtual void Union()
        => AssertQuery(new Query("oldUsers").Select("id", "name")
            .Union(new Query("users").Select("id", "name")));

    [Fact]
    public virtual void UnionAll()
        => AssertQuery(new Query("oldUsers").Select("id", "name")
            .UnionAll(new Query("users").Select("id", "name")));

    [Fact]
    public virtual void UnionCallback()
        => AssertQuery(new Query("oldUsers").Select("id", "name")
            .Union(q => new Query("users").Select("id", "name")));

    [Fact]
    public virtual void UnionAllCallback()
        => AssertQuery(new Query("oldUsers").Select("id", "name")
            .UnionAll(q => new Query("users").Select("id", "name")));

    [Fact]
    public virtual void Except()
    => AssertQuery(new Query("oldUsers").Select("id", "name")
        .Except(new Query("users").Select("id", "name")));

    [Fact]
    public virtual void ExceptAll()
        => AssertQuery(new Query("oldUsers").Select("id", "name")
            .ExceptAll(new Query("users").Select("id", "name")));

    [Fact]
    public virtual void ExceptCallback()
        => AssertQuery(new Query("oldUsers").Select("id", "name")
            .Except(q => new Query("users").Select("id", "name")));

    [Fact]
    public virtual void ExceptAllCallback()
        => AssertQuery(new Query("oldUsers").Select("id", "name")
            .ExceptAll(q => new Query("users").Select("id", "name")));

    [Fact]
    public virtual void Intersect()
    => AssertQuery(new Query("oldUsers").Select("id", "name")
        .Intersect(new Query("users").Select("id", "name")));

    [Fact]
    public virtual void IntersectAll()
        => AssertQuery(new Query("oldUsers").Select("id", "name")
            .IntersectAll(new Query("users").Select("id", "name")));

    [Fact]
    public virtual void IntersectCallback()
        => AssertQuery(new Query("oldUsers").Select("id", "name")
            .Intersect(q => new Query("users").Select("id", "name")));

    [Fact]
    public virtual void IntersectAllCallback()
        => AssertQuery(new Query("oldUsers").Select("id", "name")
            .IntersectAll(q => new Query("users").Select("id", "name")));
}
