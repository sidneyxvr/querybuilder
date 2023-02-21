using Argon.QueryBuilder.Clauses;

namespace Argon.QueryBuilder.Compilers;

public abstract partial class Compiler
{
    protected virtual string ParameterPrefix { get; set; } = "@p";
    protected virtual string OpeningIdentifier { get; set; } = "\"";
    protected virtual string ClosingIdentifier { get; set; } = "\"";
    protected virtual string ColumnAsKeyword { get; set; } = "AS ";
    protected virtual string TableAsKeyword { get; set; } = "AS ";
    protected virtual string EscapeCharacter { get; set; } = "\\";

    /// <summary>
    /// Whether the compiler supports the `SELECT ... FILTER` syntax
    /// </summary>
    /// <value></value>            
    public virtual bool SupportsFilterClause { get; set; } = false;

    /// <summary>
    /// If true the compiler will remove the SELECT clause for the query used inside WHERE EXISTS
    /// </summary>
    /// <value></value>            
    public virtual bool OmitSelectInsideExists { get; set; } = true;

    protected virtual string? SingleRowDummyTableName { get => null; }

    /// <summary>
    /// A list of white-listed operators
    /// </summary>
    /// <value></value>
    protected static readonly HashSet<string> Operators = new()
    {
        "=", "<", ">", "<=", ">=", "<>", "!=", "<=>",
        "like", "not like",
        "ilike", "not ilike",
        "like binary", "not like binary",
        "rlike", "not rlike",
        "regexp", "not regexp",
        "similar to", "not similar to"
    };

    protected HashSet<string> UserOperators = new();

    private static Query TransformAggregateQuery(Query query)
    {
        var clause = query.GetOneComponent<AggregateClause>(Component.Aggregate)!;

        if (clause.Columns.Count == 1 && !query.IsDistinct) return query;

        if (query.IsDistinct)
        {
            query.ClearComponent(Component.Aggregate);
            query.ClearComponent(Component.Select);
            query.Select(clause.Columns.ToArray());
        }
        else
        {
            foreach (var column in clause.Columns)
            {
                query.WhereNotNull(column);
            }
        }

        var outerClause = new AggregateClause()
        {
            Columns = new List<string> { "*" },
            Type = clause.Type
        };

        return new Query()
            .AddComponent(Component.Aggregate, outerClause)
            .From(query, $"{clause.Type}Query");
    }

    protected virtual SqlResult CompileRaw(Query query)
    {
        SqlResult? ctx;

        if (query.Method == "aggregate")
        {
            query.ClearComponent(Component.Limit)
                .ClearComponent(Component.Order)
                .ClearComponent(Component.Group);

            query = TransformAggregateQuery(query);
        }

        ctx = CompileQuery(query);

        // handle CTEs
        if (query.HasComponent(Component.Cte))
        {
            ctx = CompileCteQuery(ctx, query);
        }

        //ctx.RawSql = Helper.ExpandParameters(ctx.RawSql, ParameterPlaceholder, ctx.Bindings.ToArray());

        return ctx;
    }

    /// <summary>
    /// Add the passed operator(s) to the white list so they can be used with
    /// the Where/Having methods, this prevent passing arbitrary operators
    /// that opens the door for SQL injections.
    /// </summary>
    /// <param name="operators"></param>
    /// <returns></returns>
    public Compiler Whitelist(params string[] operators)
    {
        foreach (var op in operators)
        {
            UserOperators.Add(op);
        }

        return this;
    }

    public virtual SqlResult Compile(Query query)
        => CompileRaw(query);

    protected virtual SqlResult CompileQuery(Query query, SqlResult? ctx = null)
    {
        ctx ??= new SqlResult();
        ctx.Query = query.Clone();

        CompileColumns(ctx);
        CompileFrom(ctx);
        CompileJoins(ctx);
        CompileWheres(ctx);
        CompileGroups(ctx);
        CompileHaving(ctx);
        CompileOrders(ctx);
        CompileLimit(ctx);
        CompileUnion(ctx);

        return ctx;
    }

    protected virtual SqlResult CompileAdHocQuery(AdHocTableFromClause adHoc)
    {
        var ctx = new SqlResult();
        //var rowBuilder = new StringBuilder()
        //    .Append("SELECT ");

        ////    .Append(string.Join(", ", adHoc.Columns.Select(col => $"{ParameterPlaceholder} AS {Wrap(col)}")));

        //var fromTable = SingleRowDummyTableName;

        //if (fromTable != null)
        //{
        //    rowBuilder.Append(" FROM {fromTable}");
        //}

        //var rows = string.Join(" UNION ALL ", Enumerable.Repeat(rowBuilder.ToString(), adHoc.Values.Count / adHoc.Columns.Count));

        //ctx.RawSql = rows;
        //ctx.Bindings = adHoc.Values;

        return ctx;
    }

    protected virtual SqlResult CompileCteQuery(SqlResult ctx, Query query)
    {
        //var cteFinder = new CteFinder(query, EngineCode);
        //var cteSearchResult = cteFinder.Find();

        //var rawSql = new StringBuilder("WITH ");
        //var cteBindings = new List<object>();

        //foreach (var cte in cteSearchResult)
        //{
        //    var cteCtx = CompileCte(cte);

        //    cteBindings.AddRange(cteCtx.Bindings);
        //    rawSql.Append(cteCtx.RawSql.Trim());
        //    rawSql.Append(",\n");
        //}

        //rawSql.Length -= 2; // remove last comma
        //rawSql.Append('\n');
        //rawSql.Append(ctx.RawSql);

        //ctx.Bindings.InsertRange(0, cteBindings);
        //ctx.RawSql = rawSql.ToString();

        return ctx;
    }

    /// <summary>
    /// Compile a single column clause
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public virtual void CompileColumn(SqlResult ctx, AbstractColumn column)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(column);

        if (column is RawColumn raw)
        {
            foreach (var binding in raw.Bindings)
            {
                var paramName = ctx.GetParamName();
                ctx.NamedBindings.Add(paramName, binding);
            }

            ctx.SqlBuilder.Append(raw.Expression);

            return;
        }

        if (column is QueryColumn queryColumn)
        {
            ctx.SqlBuilder.Append('(');

            CompileQuery(queryColumn.Query, ctx);

            ctx.SqlBuilder.Append(')');

            if (!string.IsNullOrEmpty(queryColumn.Query.QueryAlias))
            {
                ctx.SqlBuilder.Append(' ')
                    .Append(ColumnAsKeyword)
                    .Append(WrapValue(queryColumn.Query.QueryAlias));
            }
        }

        if (column is AggregatedColumn aggregatedColumn)
        {
            //string agg = aggregatedColumn.Aggregate.ToUpperInvariant();

            //var (col, alias) = SplitAlias(CompileColumn(ctx, aggregatedColumn.Column));

            //alias = string.IsNullOrEmpty(alias) ? string.Empty : $" {ColumnAsKeyword}{alias}";

            //var filterCondition = CompileFilterConditions(ctx, aggregatedColumn);

            //if (string.IsNullOrEmpty(filterCondition))
            //{
            //    return $"{agg}({col}){alias}";
            //}

            //if (SupportsFilterClause)
            //{
            //    return $"{agg}({col}) FILTER (WHERE {filterCondition}){alias}";
            //}

            //return $"{agg}(CASE WHEN {filterCondition} THEN {col} END){alias}";
        }

        ctx.SqlBuilder.Append(Wrap(((Column)column).Name));
    }

    protected virtual void CompileFilterConditions(SqlResult ctx, AggregatedColumn aggregatedColumn)
    {
        if (aggregatedColumn.Filter == null)
        {
            return;
        }

        var wheres = aggregatedColumn.Filter.GetComponents<AbstractCondition>(Component.Where);

        CompileConditions(ctx, wheres);
    }

    public virtual SqlResult CompileCte(AbstractFrom cte)
    {
        var ctx = new SqlResult();

        if (null == cte)
        {
            return ctx;
        }

        //if (cte is RawFromClause raw)
        //{
        //    ctx.Bindings.AddRange(raw.Bindings);
        //    ctx.RawSql = $"{WrapValue(raw.Alias!)} AS ({raw.Expression})";
        //}
        //else if (cte is QueryFromClause queryFromClause)
        //{
        //    var subCtx = CompileSelectQuery(queryFromClause.Query);
        //    ctx.Bindings.AddRange(subCtx.Bindings);

        //    ctx.RawSql = $"{WrapValue(queryFromClause.Alias!)} AS ({subCtx.RawSql})";
        //}
        //else if (cte is AdHocTableFromClause adHoc)
        //{
        //    var subCtx = CompileAdHocQuery(adHoc);
        //    ctx.Bindings.AddRange(subCtx.Bindings);

        //    ctx.RawSql = $"{WrapValue(adHoc.Alias!)} AS ({subCtx.RawSql})";
        //}

        return ctx;
    }

    protected virtual SqlResult OnBeforeSelect(SqlResult ctx)
        => ctx;

    protected virtual void CompileColumns(SqlResult ctx)
    {
        ctx.SqlBuilder.Append("SELECT ");

        if (ctx.Query.IsDistinct)
        {
            ctx.SqlBuilder.Append("DISTINCT ");
        }

        if (ctx.Query.HasComponent(Component.Aggregate))
        {
            var aggregate = ctx.Query.GetOneComponent<AggregateClause>(Component.Aggregate)!;

            if (aggregate.Columns.Count == 1)
            {
                ctx.SqlBuilder.Append(aggregate.Type.ToUpperInvariant())
                    .Append('(');

                CompileColumn(ctx, new Column { Name = aggregate.Columns[0] });

                ctx.SqlBuilder.Append(") ")
                    .Append(ColumnAsKeyword)
                    .Append(WrapValue(aggregate.Type));
            }

            ctx.SqlBuilder.Append('1');
        }

        var columns = ctx.Query.GetComponents<AbstractColumn>(Component.Select);

        if (columns.Count > 0)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                if (i != 0)
                {
                    ctx.SqlBuilder.Append(", ");
                }

                CompileColumn(ctx, columns[i]);
            }
        }
        else
        {
            ctx.SqlBuilder.Append('*');
        }
    }

    public virtual void CompileUnion(SqlResult ctx)
    {
        // Handle UNION, EXCEPT and INTERSECT
        if (!ctx.Query.GetComponents(Component.Combine).Any())
        {
            return;
        }

        var clauses = ctx.Query.GetComponents<AbstractCombine>(Component.Combine);

        foreach (var clause in clauses)
        {
            switch (clause)
            {
                case Combine combineClause:
                    ctx.SqlBuilder.Append(' ').Append(combineClause.Operation.ToUpperInvariant())
                        .Append(combineClause.All ? " ALL " : " ");

                    CompileQuery(combineClause.Query, ctx);
                    break;
                case RawCombine rawCombine:

                    foreach (var binding in rawCombine.Bindings)
                    {
                        var paramName = ctx.GetParamName();
                        ctx.NamedBindings.Add(paramName, binding);
                    }

                    ctx.SqlBuilder.Append(' ').Append(rawCombine.Expression);
                    break;
                default:
                    throw new InvalidOperationException("TODO: ");
            }
        }
    }

    public virtual void CompileTableExpression(SqlResult ctx, AbstractFrom from)
    {
        switch (from)
        {
            case RawFromClause raw:
                foreach (var binding in raw.Bindings)
                {
                    var paramName = ctx.GetParamName();
                    ctx.NamedBindings.Add(paramName, binding);
                }

                ctx.SqlBuilder.Append(raw.Expression);
                break;
            case QueryFromClause queryFromClause:
                var fromQuery = queryFromClause.Query;

                ctx.SqlBuilder.Append('(');

                CompileQuery(fromQuery, ctx);

                ctx.SqlBuilder.Append(')');

                if (!string.IsNullOrEmpty(fromQuery.QueryAlias))
                {
                    ctx.SqlBuilder.Append(' ')
                        .Append(TableAsKeyword)
                        .Append(WrapValue(fromQuery.QueryAlias));
                }
                break;
            case FromClause fromClause:
                ctx.SqlBuilder.Append(Wrap(fromClause.Table));
                break;
            default:
                throw InvalidClauseException("TableExpression", from);
        }
    }

    public virtual void CompileFrom(SqlResult ctx)
    {
        if (!ctx.Query.HasComponent(Component.From))
        {
            return;
        }

        var from = ctx.Query.GetOneComponent<AbstractFrom>(Component.From)!;

        ctx.SqlBuilder.Append(" FROM ");

        CompileTableExpression(ctx, from);
    }

    public virtual void CompileJoins(SqlResult ctx)
    {
        if (!ctx.Query.HasComponent(Component.Join))
        {
            return;
        }

        var joins = ctx.Query.GetComponents<BaseJoin>(Component.Join);

        for (var i = 0; i < joins.Count; i++)
        {
            CompileJoin(ctx, joins[i].Join);
        }
    }

    public virtual void CompileJoin(SqlResult ctx, Join join, bool isNested = false)
    {
        var from = join.GetOneComponent<AbstractFrom>(Component.From)!;
        var conditions = join.GetComponents<AbstractCondition>(Component.Where);

        if (conditions.Count == 0)
        {
            return;
        }

        ctx.SqlBuilder
            .Append(' ')
            .Append(join.Type)
            .Append(' ');

        CompileTableExpression(ctx, from);

        ctx.SqlBuilder
            .Append(" ON ");

        CompileConditions(ctx, conditions);
    }

    public virtual void CompileWheres(SqlResult ctx)
    {
        if (!ctx.Query.HasComponent(Component.Where))
        {
            return;
        }

        var conditions = ctx.Query.GetComponents<AbstractCondition>(Component.Where);

        if (conditions.Count == 0)
        {
            return;
        }

        ctx.SqlBuilder.Append(" WHERE ");

        CompileConditions(ctx, conditions);
    }

    public virtual void CompileGroups(SqlResult ctx)
    {
        if (!ctx.Query.HasComponent(Component.Group))
        {
            return;
        }

        var columns = ctx.Query
            .GetComponents<AbstractColumn>(Component.Group);

        if (columns.Count == 0)
        {
            return;
        }

        ctx.SqlBuilder.Append("GROUP BY ");

        for (var i = 0; i < columns.Count; i++)
        {
            if (i != 0)
            {
                ctx.SqlBuilder.Append(", ");
            }

            ctx.SqlBuilder.Append(columns[i]);
        }
    }

    public virtual void CompileOrders(SqlResult ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        if (!ctx.Query.HasComponent(Component.Order))
        {
            return;
        }

        var columns = ctx.Query
            .GetComponents<AbstractOrderBy>(Component.Order)
            .Select(x =>
            {

                if (x is RawOrderBy raw)
                {
                    foreach (var binding in raw.Bindings)
                    {
                        var paramName = ctx.GetParamName();
                        ctx.NamedBindings.Add(paramName, binding);
                    }
                    return raw.Expression;
                }

                if (x is not OrderBy orderBy)
                {
                    throw new InvalidOperationException("TODO: ");
                }

                var direction = orderBy.Ascending ? "" : " DESC";

                return Wrap(orderBy.Column) + direction;
            });

        //TODO: "ORDER BY " + string.Join(", ", columns);
    }

    public virtual string? CompileHaving(SqlResult ctx)
    {
        if (!ctx.Query.HasComponent(Component.Having))
        {
            return null;
        }

        var sql = new List<string>();
        string boolOperator;

        var having = ctx.Query.GetComponents(Component.Having)
            .Cast<AbstractCondition>()
            .ToList();

        for (var i = 0; i < having.Count; i++)
        {
            CompileCondition(ctx, having[i]);

            boolOperator = i > 0 ? having[i].IsOr ? "OR " : "AND " : "";

            //sql.Add(boolOperator + compiled);
        }

        return $"HAVING {string.Join(" ", sql)}";
    }

    public virtual void CompileLimit(SqlResult ctx)
    {
        var limit = ctx.Query.GetLimit();
        var offset = ctx.Query.GetOffset();

        if (limit == 0 && offset == 0)
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
            var paramName = ctx.GetParamName();

            ctx.NamedBindings.Add(paramName, offset);
            ctx.SqlBuilder.Append(" LIMIT 18446744073709551615 OFFSET ")
                .Append(paramName);

            return;
        }

        var limitParamName = ctx.GetParamName();
        var offsetParamName = ctx.GetParamName();

        ctx.NamedBindings.Add(limitParamName, limit);
        ctx.NamedBindings.Add(offsetParamName, offset);

        ctx.SqlBuilder.Append(" LIMIT ")
            .Append(limitParamName)
            .Append(" OFFSET ")
            .Append(offsetParamName);
    }

    /// <summary>
    /// Compile the random statement into SQL.
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    protected virtual string CompileRandom(string seed)
        => "RANDOM()";

    protected virtual string CompileLower(string value)
        => $"LOWER({value})";

    protected virtual string CompileUpper(string value)
        => $"UPPER({value})";

    protected virtual string CompileTrue()
        => "true";

    protected virtual string CompileFalse()
        => "false";

    private static InvalidCastException InvalidClauseException(string section, AbstractClause clause)
        => new($"Invalid type \"{clause.GetType().Name}\" provided for the \"{section}\" clause.");

    protected string CheckOperator(string operation)
    {
        var op = operation.ToLowerInvariant();

        var valid = Operators.Contains(op) || UserOperators.Contains(op);

        if (!valid)
        {
            throw new InvalidOperationException($"The operator '{op}' cannot be used. Please consider white listing it before using it.");
        }

        return operation;
    }

    /// <summary>
    /// Wrap a single string in a column identifier.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual string Wrap(string value)
    {
        if (value.Contains(" as ", StringComparison.OrdinalIgnoreCase))
        {
            var (before, after) = SplitAlias(value);

            return Wrap(before) + $" {ColumnAsKeyword}" + WrapValue(after!);
        }

        if (value.Contains('.'))
        {
            return string.Join('.', value.Split('.').Select((x, index) =>
            {
                return WrapValue(x);
            }));
        }

        // If we reach here then the value does not contain an "AS" alias
        // nor dot "." expression, so wrap it as regular value.
        return WrapValue(value);
    }

    protected static (string, string?) SplitAlias(string value)
    {
        var index = value.LastIndexOf(" as ", StringComparison.OrdinalIgnoreCase);

        if (index > 0)
        {
            var before = value[..index];
            var after = value[(index + 4)..];
            return (before, after);
        }

        return (value, null);
    }

    /// <summary>
    /// Wrap a single string in keyword identifiers.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual string WrapValue(string value)
    {
        if (value == "*") return value;

        var opening = OpeningIdentifier;
        var closing = ClosingIdentifier;

        return opening + value.Replace(closing, closing + closing) + closing;
    }

    /// <summary>
    /// Resolve a parameter
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    protected virtual object Resolve(SqlResult ctx, object parameter)
    {
        // if we face a literal value we have to return it directly
        if (parameter is UnsafeLiteral literal)
        {
            return literal.Value;
        }

        // if we face a variable we have to lookup the variable from the predefined variables
        if (parameter is Variable variable)
        {
            var value = ctx.Query.FindVariable(variable.Name);
            return value;
        }

        return parameter;
    }

    /// <summary>
    /// Resolve a parameter and add it to the binding list
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    protected virtual string Parameter(SqlResult ctx, object parameter)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(parameter);

        // if we face a literal value we have to return it directly
        if (parameter is UnsafeLiteral literal)
        {
            return literal.Value;
        }

        var paramName = ctx.GetParamName();

        // if we face a variable we have to lookup the variable from the predefined variables
        if (parameter is Variable variable)
        {
            var value = ctx.Query.FindVariable(variable.Name);
            ctx.NamedBindings.Add(paramName, value);
            return paramName;
        }

        ctx.NamedBindings.Add(paramName, parameter);
        return paramName;
    }

    /// <summary>
    /// Create query parameter place-holders for an array.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    protected virtual string Parameterize<T>(SqlResult ctx, IEnumerable<T> values)
        where T : notnull
        => string.Join(", ", values.Select(x => Parameter(ctx, x)));

    /// <summary>
    /// Wrap an array of values.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    protected virtual List<string> WrapArray(List<string> values)
        => values.Select(x => Wrap(x)).ToList();
}
