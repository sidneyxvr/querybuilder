using Argon.QueryBuilder;
using Argon.QueryBuilder.MySql;
using System.Runtime.CompilerServices;
using Xunit;

namespace Argon.QueryBuilder.Tests;

public abstract class TestBase
{
    private readonly MySqlCompiler _compiler = new();
    private readonly Dictionary<string, SqlResult> _compiledQueries = new();

    protected void AssertQuery(
        Query query,
        [CallerMemberName] string testMethodName = "")
    {
        var compiledQuery = _compiler.Compile(query);

        _compiledQueries.Add(testMethodName, compiledQuery);
    }

    protected void AssertSql(
        string sql,
        [CallerMemberName] string testMethodName = "")
    {
        var query = _compiledQueries.GetValueOrDefault(testMethodName)!;

        Assert.Equal(sql, query.SqlBuilder.ToString());
    }
}
