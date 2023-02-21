using Argon.QueryBuilder.Clauses;
using System.Reflection;

namespace Argon.QueryBuilder.Compilers;

public partial class Compiler
{
    private static readonly Dictionary<string, MethodInfo> _methods = new();

    protected virtual void CompileCondition(SqlResult ctx, AbstractCondition clause)
    {
        switch (clause)
        {
            case BasicStringCondition basicStringCondition:
                CompileBasicStringCondition(ctx, basicStringCondition);
                break;
            case BasicDateCondition basicDateCondition:
                CompileBasicDateCondition(ctx, basicDateCondition);
                break;
            case BasicCondition basicCondition:
                CompileBasicCondition(ctx, basicCondition);
                break;
            case TwoColumnsCondition twoColumnsCondition:
                CompileTwoColumnsCondition(ctx, twoColumnsCondition);
                break;
            case BooleanCondition booleanCondition:
                CompileBooleanCondition(ctx, booleanCondition);
                break;
            case NullCondition nullCondition:
                CompileNullCondition(ctx, nullCondition);
                break;
            case ExistsCondition existsCondition:
                CompileExistsCondition(ctx, existsCondition);
                break;
            case InCondition<int> inCondition:
                CompileInCondition(ctx, inCondition);
                break;
            case BetweenCondition<DateTime> betweenCondition:
                CompileBetweenCondition(ctx, betweenCondition);
                break;
            case QueryCondition<Query> queryCondition:
                CompileQueryCondition(ctx, queryCondition);
                break;
            case SubQueryCondition<Query> subQueryCondition:
                CompileSubQueryCondition(ctx, subQueryCondition);
                break;
            case InQueryCondition inQueryCondition:
                CompileInQueryCondition(ctx, inQueryCondition);
                break;
            default:
                var clauseType = clause.GetType();

                var methodName = clauseType switch
                {
                    _ when clauseType.IsGenericType && clauseType.GetGenericTypeDefinition() == typeof(InCondition<>) => nameof(CompileInCondition),
                    _ when clauseType.IsGenericType && clauseType.GetGenericTypeDefinition() == typeof(BetweenCondition<>) => nameof(CompileBetweenCondition),
                    _ when clauseType.IsGenericType && clauseType.GetGenericTypeDefinition() == typeof(NestedCondition<>) => nameof(CompileNestedCondition),
                    _ => throw new InvalidCastException(clauseType.FullName),
                };

                if (!_methods.TryGetValue(clauseType.FullName!, out var methodInfo))
                {
                    methodInfo = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
                        .MakeGenericMethod(clause.GetType().GenericTypeArguments);

                    _methods.Add(clauseType.FullName!, methodInfo);
                }

                methodInfo.Invoke(this, new object[] { ctx, clause });
                break;
        };
    }

    protected virtual void CompileConditions(SqlResult ctx, List<AbstractCondition> conditions)
    {
        for (var i = 0; i < conditions.Count; i++)
        {
            if (i != 0)
            {
                ctx.SqlBuilder.Append(conditions[i].IsOr ? " OR " : " AND ");
            }

            CompileCondition(ctx, conditions[i]);
        }
    }

    protected virtual void CompileQueryCondition<T>(SqlResult ctx, QueryCondition<T> x) where T : BaseQuery<T>
    {
        ctx.SqlBuilder.Append(Wrap(x.Column))
            .Append(' ')
            .Append(CheckOperator(x.Operator))
            .Append(" (");

        CompileQuery(x.Query, ctx);

        ctx.SqlBuilder.Append(')');
    }

    protected virtual void CompileSubQueryCondition<T>(SqlResult ctx, SubQueryCondition<T> x) where T : BaseQuery<T>
    {
        ctx.SqlBuilder.Append('(');

        CompileQuery(x.Query, ctx);

        ctx.SqlBuilder.Append(") ")
            .Append(CheckOperator(x.Operator))
            .Append(' ')
            .Append(Parameter(ctx, x.Value));
    }

    protected virtual void CompileBasicCondition(SqlResult ctx, BasicCondition x)
    {
        if (x.IsNot)
        {
            ctx.SqlBuilder.Append("NOT ");
        }

        ctx.SqlBuilder.Append(Wrap(x.Column))
            .Append(' ')
            .Append(CheckOperator(x.Operator))
            .Append(' ')
            .Append(Parameter(ctx, x.Value));
    }

    protected virtual void CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x)
    {
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

        if (x.IsNot)
        {
            ctx.SqlBuilder.Append("NOT (");
        }

        ctx.SqlBuilder.Append(Wrap(x.Column))
            .Append(' ')
            .Append(CheckOperator(method))
            .Append(' ')
            .Append(x.Value is UnsafeLiteral ? value : Parameter(ctx, value));

        if (!string.IsNullOrEmpty(x.EscapeCharacter))
        {
            ctx.SqlBuilder.Append(" ESCAPE ")
                .Append('\'')
                .Append(x.EscapeCharacter)
                .Append('\'');
        }

        if (x.IsNot)
        {
            ctx.SqlBuilder.Append(") ");
        }
    }

    protected virtual void CompileBasicDateCondition(SqlResult ctx, BasicDateCondition x)
    {
        var column = Wrap(x.Column);
        var op = CheckOperator(x.Operator);

        if (x.IsNot)
        {
            ctx.SqlBuilder.Append("NOT (");
        }

        ctx.SqlBuilder.Append(x.Part.ToUpperInvariant())
            .Append('(')
            .Append(column)
            .Append(") ")
            .Append(op)
            .Append(' ')
            .Append(Parameter(ctx, x.Value));

        if (x.IsNot)
        {
            ctx.SqlBuilder.Append(") ");
        }
    }

    protected virtual void CompileNestedCondition<Q>(SqlResult ctx, NestedCondition<Q> x) where Q : BaseQuery<Q>
    {
        if (!(x.Query.HasComponent(Component.Where) || x.Query.HasComponent(Component.Having)))
        {
            return;
        }

        var clause = x.Query.HasComponent(Component.Where) ? Component.Where : Component.Having;

        var clauses = x.Query.GetComponents<AbstractCondition>(clause);

        if (x.IsNot)
        {
            ctx.SqlBuilder.Append("NOT ");
        }

        ctx.SqlBuilder.Append('(');

        CompileConditions(ctx, clauses);

        ctx.SqlBuilder.Append(')');
    }

    protected void CompileTwoColumnsCondition(SqlResult ctx, TwoColumnsCondition clause)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(clause);

        if (clause.IsNot)
        {
            ctx.SqlBuilder.Append("NOT ");
        }

        ctx.SqlBuilder.Append(Wrap(clause.First))
            .Append(' ')
            .Append(CheckOperator(clause.Operator))
            .Append(' ')
            .Append(Wrap(clause.Second));
    }

    protected virtual void CompileBetweenCondition<T>(SqlResult ctx, BetweenCondition<T> item)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(item);

        var lower = Parameter(ctx, item.Lower!);
        var higher = Parameter(ctx, item.Higher!);

        ctx.SqlBuilder.Append(Wrap(item.Column))
            .Append(item.IsNot ? " NOT BETWEEN " : " BETWEEN ")
            .Append(lower)
            .Append(" AND ")
            .Append(higher);
    }

    protected virtual void CompileInCondition<T>(SqlResult ctx, InCondition<T> item)
        where T : notnull
    {
        if (!item.Values.Any())
        {
            ctx.SqlBuilder.Append(item.IsNot ? "1 = 1 /* NOT IN [empty list] */" : "1 = 0 /* IN [empty list] */");
            return;
        }

        ctx.SqlBuilder.Append(Wrap(item.Column))
            .Append(item.IsNot ? " NOT IN " : " IN ")
            .Append('(')
            .Append(Parameterize(ctx, item.Values))
            .Append(')');
    }

    protected virtual void CompileInQueryCondition(SqlResult ctx, InQueryCondition item)
    {
        ctx.SqlBuilder.Append(Wrap(item.Column))
            .Append(item.IsNot ? " NOT IN (" : " IN (");

        CompileQuery(item.Query, ctx);

        ctx.SqlBuilder.Append(')');
    }

    protected virtual void CompileNullCondition(SqlResult ctx, NullCondition item)
    {
        ctx.SqlBuilder.Append(Wrap(item.Column))
            .Append(item.IsNot ? " IS NOT NULL" : " IS NULL");
    }

    protected virtual void CompileBooleanCondition(SqlResult ctx, BooleanCondition item)
        => ctx.SqlBuilder.Append(Wrap(item.Column))
            .Append(item.IsNot ? " != " : " = ")
            .Append(item.Value ? CompileTrue() : CompileFalse());

    protected virtual void CompileExistsCondition(SqlResult ctx, ExistsCondition item)
    {
        ctx.SqlBuilder.Append(item.IsNot ? "NOT EXISTS " : "EXISTS ");

        // remove unneeded components
        var query = item.Query.Clone();

        if (OmitSelectInsideExists)
        {
            query.ClearComponent(Component.Select).SelectRaw("1");
        }

        ctx.SqlBuilder.Append('(');

        CompileQuery(query, ctx);

        //foreach (var binding in subCtx.NamedBindings)
        //{
        //    var paramName = ctx.GetParamName();
        //    ctx.NamedBindings.Add(paramName, binding);
        //}

        ctx.SqlBuilder
            //.Append(subCtx.RawSql)
            .Append(')');
    }
}
