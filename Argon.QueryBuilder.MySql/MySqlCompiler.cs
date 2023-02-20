using Argon.QueryBuilder.Compilers;

namespace Argon.QueryBuilder.MySql;

public class MySqlCompiler : Compiler
{
    public MySqlCompiler()
    {
        OpeningIdentifier = ClosingIdentifier = "`";
    }

    public override string EngineCode { get; } = EngineCodes.MySql;

    public override void CompileLimit(SqlResult ctx)
    {
        var limit = ctx.Query.GetLimit(EngineCode);
        var offset = ctx.Query.GetOffset(EngineCode);


        if (offset == 0 && limit == 0)
        {
            return;
        }

        if (offset == 0)
        {
            var paramName = ctx.GetParamName();

            ctx.NamedBindings.Add(paramName, limit);

            ctx.SqlBuilder.Append(" LIMIT ")
                .Append(paramName);

            return;
        }

        if (limit == 0)
        {

            // MySql will not accept offset without limit, so we will put a large number
            // to avoid this error.

            var paramName = ctx.GetParamName();

            ctx.NamedBindings.Add(paramName, offset);

            ctx.SqlBuilder.Append(" LIMIT 18446744073709551615 OFFSET ")
                .Append(paramName);

            return;
        }

        // We have both values

        var limitParamName = ctx.GetParamName();
        var offsetParamName = ctx.GetParamName();

        ctx.NamedBindings.Add(limitParamName, limit);
        ctx.NamedBindings.Add(offsetParamName, offset);

        ctx.SqlBuilder.Append(" LIMIT ")
            .Append(limitParamName)
            .Append(" OFFSET ")
            .Append(offsetParamName);
    }
}
