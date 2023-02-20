//using Zine.QueryBuilder.Clauses;
//using Zine.QueryBuilder.Exceptions;
//using System;
//using System.Text;

//namespace Argon.QueryBuilder.Compilers;

//public class FirebirdCompiler : Compiler
//{
//    public FirebirdCompiler()
//    {
//    }

//    public override string EngineCode { get; } = EngineCodes.Firebird;
//    protected override string SingleRowDummyTableName => "RDB$DATABASE";

//    public override string? CompileLimit(SqlResult ctx)
//    {
//        ArgumentNullException.ThrowIfNull(ctx);
//        CustomNullReferenceException.ThrowIfNull(ctx.Query);

//        var limit = ctx.Query.GetLimit(EngineCode);
//        var offset = ctx.Query.GetOffset(EngineCode);

//        if (limit > 0 && offset > 0)
//        {
//            ctx.Bindings.Add(offset + 1);
//            ctx.Bindings.Add(limit + offset);

//            return $"ROWS {ParameterPlaceholder} TO {ParameterPlaceholder}";
//        }

//        return null;
//    }


//    protected override string CompileColumns(SqlResult ctx)
//    {
//        var compiled = base.CompileColumns(ctx);

//        var limit = ctx.Query.GetLimit(EngineCode);
//        var offset = ctx.Query.GetOffset(EngineCode);

//        if (limit > 0 && offset == 0)
//        {
//            ctx.Bindings.Insert(0, limit);

//            ctx.Query.ClearComponent(Component.Limit);

//            return string.Concat($"SELECT FIRST {ParameterPlaceholder}", compiled[6..]);
//        }
//        else if (limit == 0 && offset > 0)
//        {
//            ctx.Bindings.Insert(0, offset);

//            ctx.Query.ClearComponent(Component.Offset);

//            return $"SELECT SKIP {ParameterPlaceholder}" + compiled[6..];
//        }

//        return compiled;
//    }

//    protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
//    {
//        var column = Wrap(condition.Column);

//        string left;

//        if (condition.Part == "time")
//        {
//            left = $"CAST({column} as TIME)";
//        }
//        else if (condition.Part == "date")
//        {
//            left = $"CAST({column} as DATE)";
//        }
//        else
//        {
//            left = $"EXTRACT({condition.Part.ToUpperInvariant()} FROM {column})";
//        }

//        var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

//        if (condition.IsNot)
//        {
//            return $"NOT ({sql})";
//        }

//        return sql;
//    }

//    public override string WrapValue(string value)
//        => base.WrapValue(value).ToUpperInvariant();

//    public override string CompileTrue()
//        => "1";

//    public override string CompileFalse()
//        => "0";
//}
