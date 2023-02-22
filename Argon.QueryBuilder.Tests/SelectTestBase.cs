using Xunit;

namespace Argon.QueryBuilder.Tests;

public class SelectTestBase : TestBase
{
    [Fact]
    public void EscapeClauseThrowsForMultipleCharacters()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
        {
            var q = new Query("Table1")
                .HavingContains("Column1", @"TestString\%", @"\aa");
        });
    }

    [Fact]
    public virtual void BasicSelect()
        => AssertQuery(new Query()
            .From("users")
            .Select("id", "name"));

    [Fact]
    public virtual void BasicSelectEnumerable()
        => AssertQuery(new Query()
            .From("users")
            .Select(new List<string>() { "id", "name" }));

    [Fact]
    public virtual void BasicSelectWhereBindingIsEmptyOrNull()
        => AssertQuery(new Query()
            .From("users")
            .Select("id", "name")
            .Where("author", "")
            .OrWhere("author", null));

    [Fact]
    public virtual void BasicSelectWithAlias()
        => AssertQuery(new Query()
        .From("users as u")
        .Select("id", "name"));

    [Fact]
    public virtual void ExpandedSelect()
        => AssertQuery(new Query()
        .From("users")
        .Select("users.{id,name, age}"));

    [Fact]
    public virtual void UnionWithBindings()
        => AssertQuery(new Query("Phones")
            .Union(new Query("Laptops").Where("Type", "A")));

    [Fact]
    public virtual void CombineRawWithPlaceholders()
        => AssertQuery(new Query("Mobiles")
        .CombineRaw("UNION ALL SELECT * FROM {Devices}"));

    //[Fact]
    //public virtual void CteAndBindings()
    //    => AssertQuery(new Query("Races")
    //        .For("mysql", s =>
    //            s.With("range", q =>
    //                    q.From("seqtbl")
    //                        .Select("Id").Where("Id", "<", 33))
    //                .WhereIn("RaceAuthor", q => q.From("Users")
    //                    .Select("Name").Where("Status", "Available")
    //                )
    //        )
    //        .Where("Id", ">", 55)
    //        .WhereBetween("Value", 18, 24));
    //Assert.Equal(
    //    "WITH `range` AS (SELECT `Id` FROM `seqtbl` WHERE `Id` < 33)\nSELECT * FROM `Races` WHERE `RaceAuthor` IN (SELECT `Name` FROM `Users` WHERE `Status` = 'Available') AND `Id` > 55 AND `Value` BETWEEN 18 AND 24",
    //    c[EngineCodes.MySql]);

    // test for issue #50
    [Fact]
    public virtual void CascadedCteAndBindings()
    {
        var cte1 = new Query("Table1");
        cte1.Select("Column1", "Column2");
        cte1.Where("Column2", 1);

        var cte2 = new Query("Table2");
        cte2.With("cte1", cte1);
        cte2.Select("Column3", "Column4");
        cte2.Join("cte1", join => join.On("Column1", "Column3"));
        cte2.Where("Column4", 2);

        var mainQuery = new Query("Table3");
        mainQuery.With("cte2", cte2);
        mainQuery.Select("*");
        mainQuery.From("cte2");
        mainQuery.Where("Column3", 5);
    }

    // test for issue #50
    [Fact]
    public virtual void CascadedAndMultiReferencedCteAndBindings()
    {
        var cte1 = new Query("Table1");
        cte1.Select("Column1", "Column2");
        cte1.Where("Column2", 1);

        var cte2 = new Query("Table2");
        cte2.With("cte1", cte1);
        cte2.Select("Column3", "Column4");
        cte2.Join("cte1", join => join.On("Column1", "Column3"));
        cte2.Where("Column4", 2);

        var cte3 = new Query("Table3");
        cte3.With("cte1", cte1);
        cte3.Select("Column3_3", "Column3_4");
        cte3.Join("cte1", join => join.On("Column1", "Column3_3"));
        cte3.Where("Column3_4", 33);

        var mainQuery = new Query("Table3");
        mainQuery.With("cte2", cte2);
        mainQuery.With("cte3", cte3);
        mainQuery.Select("*");
        mainQuery.From("cte2");
        mainQuery.Where("Column3", 5);
    }

    // test for issue #50
    [Fact]
    public virtual void MultipleCtesAndBindings()
    {
        var cte1 = new Query("Table1");
        cte1.Select("Column1", "Column2");
        cte1.Where("Column2", 1);

        var cte2 = new Query("Table2");
        cte2.Select("Column3", "Column4");
        cte2.Join("cte1", join => join.On("Column1", "Column3"));
        cte2.Where("Column4", 2);

        var cte3 = new Query("Table3");
        cte3.Select("Column3_3", "Column3_4");
        cte3.Join("cte1", join => join.On("Column1", "Column3_3"));
        cte3.Where("Column3_4", 33);

        var mainQuery = new Query("Table3");
        mainQuery.With("cte1", cte1);
        mainQuery.With("cte2", cte2);
        mainQuery.With("cte3", cte3);
        mainQuery.Select("*");
        mainQuery.From("cte3");
        mainQuery.Where("Column3_4", 5);
    }

    [Fact]
    public virtual void Limit()
        => AssertQuery(new Query()
        .From("users")
        .Select("id", "name")
        .Limit(10));

    [Fact]
    public virtual void Offset()
        => AssertQuery(new Query()
        .From("users")
        .Offset(10));

    [Fact]
    public virtual void LimitOffset()
        => AssertQuery(new Query()
        .From("users")
        .Offset(10)
        .Limit(5));

    [Fact]
    public virtual void BasicJoin()
        => AssertQuery(new Query()
        .From("users")
        .Join("countries", "countries.id", "users.country_id"));

    [Theory]
    [InlineData("inner join", "INNER JOIN")]
    [InlineData("left join", "LEFT JOIN")]
    [InlineData("right join", "RIGHT JOIN")]
    [InlineData("cross join", "CROSS JOIN")]
    public virtual void JoinTypes(string given, string output)
        => AssertQuery(new Query()
        .From("users")
        .Join("countries", "countries.id", "users.country_id", "=", given));


    [Fact]
    public virtual void SelectSum()
        => AssertQuery(new Query()
        .From("users")
        .SelectSum("value"));

    [Fact]
    public virtual void SelectSumWithAlias()
        => AssertQuery(new Query()
        .From("users")
        .SelectSum("value AS Total"));

    [Fact]
    public virtual void SelectRawWithBindings()
        => AssertQuery(new Query()
        .From("users")
        .SelectRaw("id, ?, name", 1));
}
