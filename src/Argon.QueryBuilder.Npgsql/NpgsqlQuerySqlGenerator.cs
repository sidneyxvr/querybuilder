using Argon.QueryBuilder.Clauses;

namespace Argon.QueryBuilder.Npgsql;

public class NpgsqlQuerySqlGenerator : QuerySqlGenerator, IQuerySqlGenerator
{
    public static SqlResult Compile(Query query)
        => new NpgsqlQuerySqlGenerator().CompileQuery(query);

    protected override void VisitBasicStringCondition(BasicStringCondition x)
    {
        //var column = Wrap(x.Column);

        //var value = Resolve(ctx, x.Value) as string;

        //if (value == null)
        //{
        //    throw new ArgumentException("Expecting a non nullable string");
        //}

        //var method = x.Operator;

        //if (new[] { "starts", "ends", "contains", "like", "ilike" }.Contains(x.Operator))
        //{
        //    method = x.CaseSensitive ? "LIKE" : "ILIKE";

        //    switch (x.Operator)
        //    {
        //        case "starts":
        //            value = $"{value}%";
        //            break;
        //        case "ends":
        //            value = $"%{value}";
        //            break;
        //        case "contains":
        //            value = $"%{value}%";
        //            break;
        //    }
        //}

        //string sql;

        //if (x.Value is UnsafeLiteral)
        //{
        //    sql = $"{column} {checkOperator(method)} {value}";
        //}
        //else
        //{
        //    sql = $"{column} {checkOperator(method)} {Parameter(ctx, value)}";
        //}

        //if (!string.IsNullOrEmpty(x.EscapeCharacter))
        //{
        //    sql = $"{sql} ESCAPE '{x.EscapeCharacter}'";
        //}

        //x.IsNot ? $"NOT ({sql})" : sql;
    }

    protected override void VisitBasicDateCondition(BasicDateCondition x)
    {
        //var column = Wrap(condition.Column);

        //string left;

        //if (condition.Part == "time")
        //{
        //    left = $"{column}::time";
        //}
        //else if (condition.Part == "date")
        //{
        //    left = $"{column}::date";
        //}
        //else
        //{
        //    left = $"DATE_PART('{condition.Part.ToUpperInvariant()}', {column})";
        //}

        //var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

        //if (condition.IsNot)
        //{
        //    return $"NOT ({sql})";
        //}
    }
}
