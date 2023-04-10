using Argon.QueryBuilder.Clauses;

namespace Argon.QueryBuilder.MySql;

public sealed class MySqlQuerySqlGenerator : QuerySqlGenerator, IQuerySqlGenerator
{
    static MySqlQuerySqlGenerator()
        => OpeningIdentifier = ClosingIdentifier = "`";

    public static SqlResult Compile(Query query)
        => new MySqlQuerySqlGenerator().CompileQuery(query);

    public override void VisitLimitOffset(LimitClause? limitClause, OffsetClause? offsetClause)
    {
        var limit = limitClause?.Limit ?? 0;
        var offset = offsetClause?.Offset ?? 0;

        if (offset == 0 && limit == 0)
        {
            return;
        }

        if (offset == 0)
        {
            SqlBuilder.Append(" LIMIT ")
                .Append(Parameter(limit));

            return;
        }

        if (limit == 0)
        {
            // MySql will not accept offset without limit, so we will put a large number
            // to avoid this error.
            SqlBuilder.Append(" LIMIT 18446744073709551615 OFFSET ")
                .Append(Parameter(offset));

            return;
        }

        // We have both values
        SqlBuilder.Append(" LIMIT ")
            .Append(Parameter(limit))
            .Append(" OFFSET ")
            .Append(Parameter(offset));
    }

    protected override string DbValueTrue()
        => "1";

    protected override string DbValueFalse()
        => "0";
}
