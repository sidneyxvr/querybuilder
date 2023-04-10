using Argon.QueryBuilder.MySql;
using System.Runtime.CompilerServices;
using Xunit;

namespace Argon.QueryBuilder.Tests;

public abstract class TestBase
{
    private readonly Dictionary<string, SqlResult> _compiledQueries = new();

    protected void AssertQuery(
        Query query,
        [CallerMemberName] string testMethodName = "")
    {
        var compiledQuery = MySqlQuerySqlGenerator.Compile(query);

        _compiledQueries.Add(testMethodName, compiledQuery);
    }

    protected void AssertSql(
        string sql,
        (string, object)[]? parameters = null,
        [CallerMemberName] string testMethodName = "")
    {
        var query = _compiledQueries.GetValueOrDefault(testMethodName)!;

        Assert.Equal(sql, query.Sql);
        if (parameters?.Any() == true)
        {
            Assert.Equal(parameters.ToArray(), query.Parameters.Select(p => (p.Key, p.Value)).ToArray());
        }
    }
}
