using Argon.QueryBuilder.Clauses;

namespace Argon.QueryBuilder.Compilers;

public partial class Compiler
{
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
            default:
                throw new Exception();
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

    protected virtual string CompileRawCondition(SqlResult ctx, RawCondition x)
    {
        foreach (var binding in x.Bindings)
        {
            var paramName = ctx.GetParamName();
            ctx.NamedBindings.Add(paramName, binding);
        }

        return x.Expression;
    }

    protected virtual string CompileQueryCondition<T>(SqlResult ctx, QueryCondition<T> x) where T : BaseQuery<T>
    {
        var subCtx = CompileQuery(x.Query);

        foreach (var binding in subCtx.NamedBindings)
        {
            var paramName = ctx.GetParamName();
            ctx.NamedBindings.Add(paramName, binding);
        }

        return Wrap(x.Column) + " " + CheckOperator(x.Operator) + " (" + subCtx.RawSql + ")";
    }

    protected virtual string CompileSubQueryCondition<T>(SqlResult ctx, SubQueryCondition<T> x) where T : BaseQuery<T>
    {
        var subCtx = CompileQuery(x.Query);

        foreach (var binding in subCtx.NamedBindings)
        {
            var paramName = ctx.GetParamName();
            ctx.NamedBindings.Add(paramName, binding);
        }

        return "(" + subCtx.RawSql + ") " + CheckOperator(x.Operator) + " " + Parameter(ctx, x.Value);
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


        if (!x.CaseSensitive)
        {
            column = CompileLower(column);
            value = value.ToLowerInvariant();
        }

        if (x.IsNot)
        {
            ctx.SqlBuilder.Append("NOT (");
        }

        if (x.Value is UnsafeLiteral)
        {
            ctx.SqlBuilder.Append(column)
                .Append(' ')
                .Append(CheckOperator(method))
                .Append(' ')
                .Append(value);
        }
        else
        {
            ctx.SqlBuilder.Append(column)
                .Append(' ')
                .Append(CheckOperator(method))
                .Append(' ')
                .Append(Parameter(ctx, value));
        }

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
        if (!(x.Query.HasComponent(Component.Where, EngineCode) || x.Query.HasComponent(Component.Having, EngineCode)))
        {
            return;
        }

        var clause = x.Query.HasComponent(Component.Where, EngineCode) ? Component.Where : Component.Having;

        var clauses = x.Query.GetComponents<AbstractCondition>(clause, EngineCode);

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
        var subCtx = CompileQuery(item.Query);

        foreach (var binding in subCtx.NamedBindings)
        {
            var paramName = ctx.GetParamName();
            ctx.NamedBindings.Add(paramName, binding);
        }

        var inOperator = item.IsNot ? "NOT IN" : "IN";

        return Wrap(item.Column) + $" {inOperator} ({subCtx.RawSql})";
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

        var subCtx = CompileQuery(query);

        foreach (var binding in subCtx.NamedBindings)
        {
            var paramName = ctx.GetParamName();
            ctx.NamedBindings.Add(paramName, binding);
        }

        ctx.SqlBuilder.Append(subCtx.RawSql)
            .Append(')');
    }
}
