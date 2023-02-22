using Argon.QueryBuilder.Compilers;

namespace Argon.QueryBuilder.MySql;

public sealed class MySqlQuerySqlGenerator : QuerySqlGenerator, IQuerySqlGenerator
{
    static MySqlQuerySqlGenerator()
        => OpeningIdentifier = ClosingIdentifier = "`";

    public static SqlResult Compile(Query query)
        => new MySqlQuerySqlGenerator().VisitQuery(query);

    public override void VisitLimit(Query query)
    {
        var limit = query.GetLimit();
        var offset = query.GetOffset();

        if (offset == 0 && limit == 0)
        {
            return;
        }

        if (offset == 0)
        {
            SqlBuilder.Append(" LIMIT ")
                .Append(Parameter(query, limit));

            return;
        }

        if (limit == 0)
        {
            // MySql will not accept offset without limit, so we will put a large number
            // to avoid this error.
            SqlBuilder.Append(" LIMIT 18446744073709551615 OFFSET ")
                .Append(Parameter(query, offset));

            return;
        }

        // We have both values
        SqlBuilder.Append(" LIMIT ")
            .Append(Parameter(query, limit))
            .Append(" OFFSET ")
            .Append(Parameter(query, offset));
    }

    protected override string CompileFalse()
        => "0";

    protected override string CompileTrue()
        => "1";
}
