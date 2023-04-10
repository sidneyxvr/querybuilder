using Xunit;

namespace Argon.QueryBuilder.Tests;

public class JoinTestBase : TestBase
{
    [Fact]
    public virtual void BasicJoin()
        => AssertQuery(new Query("users as u")
            .Join("posts as p", "p.userId", "u.id"));

    [Fact]
    public virtual void BasicJoinTableOn()
    => AssertQuery(new Query("users as u")
        .Join("posts as p",
            on => on.Where("p.userId", "u.id")));

    [Fact]
    public virtual void BasicJoinQuery()
        => AssertQuery(new Query("users as u")
            .Join(new Query("posts as p"),
                on => on.Where("p.userId", "u.id")));

    [Fact]
    public virtual void BasicJoinOrOn()
    => AssertQuery(new Query("users as u")
        .Join("posts as p", join => join.On("p.userId", "u.id")
            .OrOn("p.createdAt", "u.createdAt")));

    [Fact]
    public virtual void BasicJoinNotEqual()
        => AssertQuery(new Query("users as u")
            .Join("posts as p", "p.userId", "u.id", "!="));

    [Fact]
    public virtual void BasicLeftJoinAsParam()
        => AssertQuery(new Query("users as u")
            .Join("posts as p", "p.userId", "u.id", type: "left join"));

    [Fact]
    public virtual void BasicLeftJoin()
        => AssertQuery(new Query("users as u")
            .LeftJoin("posts as p", "p.userId", "u.id"));

    [Fact]
    public virtual void BasicLeftJoinTableOn()
        => AssertQuery(new Query("users as u")
            .LeftJoin("posts as p",
                on => on.Where("p.userId", "u.id")));

    [Fact]
    public virtual void BasicLeftJoinQuery()
        => AssertQuery(new Query("users as u")
            .LeftJoin(new Query("posts as p"),
                on => on.Where("p.userId", "u.id")));

    [Fact]
    public virtual void BasicRightJoinAsParam()
        => AssertQuery(new Query("users as u")
            .Join("posts as p", "p.userId", "u.id", type: "right join"));

    [Fact]
    public virtual void BasicRightJoin()
        => AssertQuery(new Query("users as u")
            .RightJoin("posts as p", "p.userId", "u.id"));

    [Fact]
    public virtual void BasicRightJoinTableOn()
        => AssertQuery(new Query("users as u")
            .RightJoin("posts as p",
                on => on.Where("p.userId", "u.id")));

    [Fact]
    public virtual void BasicRightJoinQuery()
        => AssertQuery(new Query("users as u")
            .RightJoin(new Query("posts as p"),
                on => on.Where("p.userId", "u.id")));
}
