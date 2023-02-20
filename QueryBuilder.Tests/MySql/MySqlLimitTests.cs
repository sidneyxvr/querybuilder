using QueryBuilder.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.MySql
{
    public class MySqlLimitTests : TestSupport
    {
        private readonly MySqlCompiler compiler;

        public MySqlLimitTests()
        {
            compiler = Compilers.Get<MySqlCompiler>(EngineCodes.MySql);
        }

        //[Fact]
        //public void WithNoLimitNorOffset()
        //{
        //    var query = new Query("Table");
        //    var ctx = new SqlResult { Query = query };

        //    Assert.Null(compiler.CompileLimit(ctx));
        //}

        //[Fact]
        //public void WithNoOffset()
        //{
        //    var query = new Query("Table").Limit(10);
        //    var ctx = new SqlResult { Query = query };

        //    Assert.Equal("LIMIT @p0", compiler.CompileLimit(ctx));
        //    Assert.Equal(10, ctx.NamedBindings.GetValueOrDefault("@p0"));
        //}

        //[Fact]
        //public void WithNoLimit()
        //{
        //    var query = new Query("Table").Offset(20);
        //    var ctx = new SqlResult { Query = query };

        //    Assert.Equal("LIMIT 18446744073709551615 OFFSET @p0", compiler.CompileLimit(ctx));
        //    Assert.Equal(20L, ctx.NamedBindings.GetValueOrDefault("@p0"));
        //    Assert.Single(ctx.NamedBindings);
        //}

        //[Fact]
        //public void WithLimitAndOffset()
        //{
        //    var query = new Query("Table").Limit(5).Offset(20);
        //    var ctx = new SqlResult { Query = query };

        //    Assert.Equal("LIMIT @p0 OFFSET @p1", compiler.CompileLimit(ctx));
        //    Assert.Equal(5, ctx.NamedBindings.GetValueOrDefault("@p0"));
        //    Assert.Equal(20L, ctx.NamedBindings.GetValueOrDefault("@p1"));
        //    Assert.Equal(2, ctx.NamedBindings.Count);
        //}
    }
}
