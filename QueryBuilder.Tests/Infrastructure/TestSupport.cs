using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests.Infrastructure;
public abstract class TestSupport
{
    protected readonly TestCompilersContainer Compilers = new();

    /// <summary>
    /// For legacy test support
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    protected IReadOnlyDictionary<string, string> Compile(Query query)
        => Compilers.Compile(query).ToDictionary(s => s.Key, v => v.Value.ToString());

    protected static void AssertSql(string expectedSql,
        IReadOnlyDictionary<string, string> queries)
    {
        Assert.Equal(expectedSql, queries[EngineCodes.SqlServer]);
        Assert.Equal(ReplaceSqlPlaceholder(expectedSql, new[] { '[', ']' }, new[] { '`', '`' } ), queries[EngineCodes.MySql]);
        Assert.Equal(expectedSql, queries[EngineCodes.PostgreSql]);
        Assert.Equal(expectedSql, queries[EngineCodes.Firebird]);
    }

    private static string ReplaceSqlPlaceholder(string sql, char[] placeholers, char[] newPlaceholders)
    {
        foreach (var (placeholer, newPlaceholder) in placeholers.Zip(newPlaceholders))
        {
            sql = sql.Replace(placeholer, newPlaceholder);
        }

        return sql;
    }
}
