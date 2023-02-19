using QueryBuilder.Clauses;
using System.Reflection;
using System.Text;

namespace SqlKata.Compilers;

public partial class Compiler
{
    protected virtual MethodInfo FindCompilerMethodInfo(Type clauseType, string methodName)
        => _compileConditionMethodsProvider.GetMethodInfo(clauseType, methodName);

    protected virtual string? CompileCondition(SqlResult ctx, AbstractCondition clause)
    {
        var clauseType = clause.GetType();

        var name = clauseType.Name;

        name = name[..name.IndexOf("Condition")];

        var methodName = "Compile" + name + "Condition";

        var methodInfo = FindCompilerMethodInfo(clauseType, methodName);

        try
        {

            var result = methodInfo.Invoke(this,
                new object[]
                {
                    ctx,
                    clause
                });

            return result as string;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to invoke '{methodName}'", ex);
        }
    }

    protected virtual string CompileConditions(SqlResult ctx, List<AbstractCondition> conditions)
    {
        var conditionsBuilder = new StringBuilder();

        for (var i = 0; i < conditions.Count; i++)
        {
            var compiled = CompileCondition(ctx, conditions[i]);

            if (string.IsNullOrEmpty(compiled))
            {
                continue;
            }

            if (i != 0)
            {
                conditionsBuilder.Append(conditions[i].IsOr ? " OR " : " AND ");
            }

            conditionsBuilder.Append(compiled);
        }

        return conditionsBuilder.ToString();
    }

    protected virtual string CompileRawCondition(SqlResult ctx, RawCondition x)
    {
        ctx.Bindings.AddRange(x.Bindings);
        return x.Expression;
    }

    protected virtual string CompileQueryCondition<T>(SqlResult ctx, QueryCondition<T> x) where T : BaseQuery<T>
    {
        var subCtx = CompileSelectQuery(x.Query);

        ctx.Bindings.AddRange(subCtx.Bindings);

        return Wrap(x.Column) + " " + CheckOperator(x.Operator) + " (" + subCtx.RawSql + ")";
    }

    protected virtual string CompileSubQueryCondition<T>(SqlResult ctx, SubQueryCondition<T> x) where T : BaseQuery<T>
    {
        var subCtx = CompileSelectQuery(x.Query);

        ctx.Bindings.AddRange(subCtx.Bindings);

        return "(" + subCtx.RawSql + ") " + CheckOperator(x.Operator) + " " + Parameter(ctx, x.Value);
    }

    protected virtual string CompileBasicCondition(SqlResult ctx, BasicCondition x)
    {
        var sql = $"{Wrap(x.Column)} {CheckOperator(x.Operator)} {Parameter(ctx, x.Value)}";

        if (x.IsNot)
        {
            return $"NOT ({sql})";
        }

        return sql;
    }

    protected virtual string CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x)
    {
        var column = Wrap(x.Column);

        if (Resolve(ctx, x.Value) is not string value)
        {
            throw new ArgumentException("Expecting a non nullable string");
        }

        var method = x.Operator;

        if (new[] { "starts", "ends", "contains", "like" }.Contains(x.Operator))
        {

            method = "LIKE";

            switch (x.Operator)
            {
                case "starts":
                    value = $"{value}%";
                    break;
                case "ends":
                    value = $"%{value}";
                    break;
                case "contains":
                    value = $"%{value}%";
                    break;
            }
        }

        string sql;


        if (!x.CaseSensitive)
        {
            column = CompileLower(column);
            value = value.ToLowerInvariant();
        }

        if (x.Value is UnsafeLiteral)
        {
            sql = $"{column} {CheckOperator(method)} {value}";
        }
        else
        {
            sql = $"{column} {CheckOperator(method)} {Parameter(ctx, value)}";
        }

        if (!string.IsNullOrEmpty(x.EscapeCharacter))
        {
            sql = $"{sql} ESCAPE '{x.EscapeCharacter}'";
        }

        return x.IsNot ? $"NOT ({sql})" : sql;

    }

    protected virtual string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition x)
    {
        var column = Wrap(x.Column);
        var op = CheckOperator(x.Operator);

        var sql = $"{x.Part.ToUpperInvariant()}({column}) {op} {Parameter(ctx, x.Value)}";

        return x.IsNot ? $"NOT ({sql})" : sql;
    }

    protected virtual string? CompileNestedCondition<Q>(SqlResult ctx, NestedCondition<Q> x) where Q : BaseQuery<Q>
    {
        if (!(x.Query.HasComponent(Component.Where, EngineCode) || x.Query.HasComponent(Component.Having, EngineCode)))
        {
            return null;
        }

        var clause = x.Query.HasComponent(Component.Where, EngineCode) ? Component.Where : Component.Having;

        var clauses = x.Query.GetComponents<AbstractCondition>(clause, EngineCode);

        var sql = CompileConditions(ctx, clauses);

        return x.IsNot ? $"NOT ({sql})" : $"({sql})";
    }

    protected string CompileTwoColumnsCondition(SqlResult ctx, TwoColumnsCondition clause)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(clause);

        var op = clause.IsNot ? "NOT " : "";
        return $"{op}{Wrap(clause.First)} {CheckOperator(clause.Operator)} {Wrap(clause.Second)}";
    }

    protected virtual string CompileBetweenCondition<T>(SqlResult ctx, BetweenCondition<T> item)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(item);

        var between = item.IsNot ? "NOT BETWEEN" : "BETWEEN";
        var lower = Parameter(ctx, item.Lower!);
        var higher = Parameter(ctx, item.Higher!);

        return Wrap(item.Column) + $" {between} {lower} AND {higher}";
    }

    protected virtual string CompileInCondition<T>(SqlResult ctx, InCondition<T> item)
        where T : notnull
    {
        var column = Wrap(item.Column);

        if (!item.Values.Any())
        {
            return item.IsNot ? $"1 = 1 /* NOT IN [empty list] */" : "1 = 0 /* IN [empty list] */";
        }

        var inOperator = item.IsNot ? "NOT IN" : "IN";

        var values = Parameterize(ctx, item.Values);

        return column + $" {inOperator} ({values})";
    }

    protected virtual string CompileInQueryCondition(SqlResult ctx, InQueryCondition item)
    {
        var subCtx = CompileSelectQuery(item.Query);

        ctx.Bindings.AddRange(subCtx.Bindings);

        var inOperator = item.IsNot ? "NOT IN" : "IN";

        return Wrap(item.Column) + $" {inOperator} ({subCtx.RawSql})";
    }

    protected virtual string CompileNullCondition(SqlResult ctx, NullCondition item)
    {
        var op = item.IsNot ? "IS NOT NULL" : "IS NULL";
        return Wrap(item.Column) + " " + op;
    }

    protected virtual string CompileBooleanCondition(SqlResult ctx, BooleanCondition item)
    {
        var column = Wrap(item.Column);
        var value = item.Value ? CompileTrue() : CompileFalse();

        var op = item.IsNot ? "!=" : "=";

        return $"{column} {op} {value}";
    }

    protected virtual string CompileExistsCondition(SqlResult ctx, ExistsCondition item)
    {
        var op = item.IsNot ? "NOT EXISTS" : "EXISTS";

        // remove unneeded components
        var query = item.Query.Clone();

        if (OmitSelectInsideExists)
        {
            query.ClearComponent(Component.Select).SelectRaw("1");
        }

        var subCtx = CompileSelectQuery(query);

        ctx.Bindings.AddRange(subCtx.Bindings);

        return $"{op} ({subCtx.RawSql})";
    }
}
