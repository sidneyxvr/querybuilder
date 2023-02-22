using Argon.QueryBuilder.Clauses;
using System.Reflection;
using System.Text;

namespace Argon.QueryBuilder.Compilers;

public interface IQuerySqlGenerator
{
    abstract static SqlResult Compile(Query query);
}

public class QuerySqlGenerator
{
    private static readonly Dictionary<string, MethodInfo> _methods = new();


    protected readonly StringBuilder SqlBuilder = new();

    protected readonly Dictionary<string, object> NamedBindings = new();

    private int index = 0;

    private string GetParamName()
        => $"@p{index++}";

    private void AddParameter(object value)
        => NamedBindings.Add(GetParamName(), value);

    private void AddParameters(object[] value)
    {
        foreach (var item in value)
        {
            AddParameter(item);
        }
    }
    protected static string OpeningIdentifier { get; set; } = "\"";
    protected static string ClosingIdentifier { get; set; } = "\"";
    protected static string ColumnAsKeyword { get; set; } = "AS ";
    protected static string TableAsKeyword { get; set; } = " AS ";
    protected static string EscapeCharacter { get; set; } = "\\";

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

    /// <summary>
    /// A list of white-listed operators
    /// </summary>
    /// <value></value>
    protected static readonly HashSet<string> Operators = new()
    {
        "=",
        "<",
        ">",
        "<=",
        ">=",
        "<>",
        "!=",
        "<=>",
        "like",
        "not like",
        "ilike",
        "not ilike",
        "like binary",
        "not like binary",
        "rlike",
        "not rlike",
        "regexp",
        "not regexp",
        "similar to",
        "not similar to"
    };

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

    protected virtual SqlResult VisitSelect(Query query)
    {
        if (query.Method == "aggregate")
        {
            query.ClearComponent(Component.Limit)
                .ClearComponent(Component.Order)
                .ClearComponent(Component.Group);

            query = TransformAggregateQuery(query);
        }

        VisitQuery(query);

        // handle CTEs
        if (query.HasComponent(Component.Cte))
        {
            //CompileCteQuery(query);
        }

        return new SqlResult
        {
            Sql = SqlBuilder.ToString(),
            Parameters = NamedBindings.AsReadOnly(),
        };
    }

    protected virtual SqlResult VisitQuery(Query query)
    {
        var currentQuery = query.Clone();

        VisitProjection(currentQuery);
        VisitFrom(currentQuery);
        VisitJoins(currentQuery);
        VisitWheres(currentQuery);
        VisitGroups(currentQuery);
        VisitHaving(currentQuery);
        VisitOrders(currentQuery);
        VisitLimit(currentQuery);
        VisitUnion(currentQuery);

        return new SqlResult
        {
            Sql = SqlBuilder.ToString(),
            Parameters = NamedBindings.AsReadOnly(),
        };
    }

    //protected virtual SqlResult CompileAdHocQuery(AdHocTableFromClause adHoc)
    //{
    //    var ctx = new SqlResult();
    //    //var rowBuilder = new StringBuilder()
    //    //    .Append("SELECT ");

    //    ////    .Append(string.Join(", ", adHoc.Columns.Select(col => $"{ParameterPlaceholder} AS {Wrap(col)}")));

    //    //var fromTable = SingleRowDummyTableName;

    //    //if (fromTable != null)
    //    //{
    //    //    rowBuilder.Append(" FROM {fromTable}");
    //    //}

    //    //var rows = string.Join(" UNION ALL ", Enumerable.Repeat(rowBuilder.ToString(), adHoc.Values.Count / adHoc.Columns.Count));

    //    //RawSql = rows;
    //    //Bindings = adHoc.Values;

    //    return ctx;
    //}

    //protected virtual SqlResult CompileCteQuery(Query query)
    //{
    //    //var cteFinder = new CteFinder(query, EngineCode);
    //    //var cteSearchResult = cteFinder.Find();

    //    //var rawSql = new StringBuilder("WITH ");
    //    //var cteBindings = new List<object>();

    //    //foreach (var cte in cteSearchResult)
    //    //{
    //    //    var cteCtx = CompileCte(cte);

    //    //    cteBindings.AddRange(cteBindings);
    //    //    rawSql.Append(cteRawSql.Trim());
    //    //    rawSql.Append(",\n");
    //    //}

    //    //rawSql.Length -= 2; // remove last comma
    //    //rawSql.Append('\n');
    //    //rawSql.Append(RawSql);

    //    //Bindings.InsertRange(0, cteBindings);
    //    //RawSql = rawSql.ToString();

    //    return ctx;
    //}

    /// <summary>
    /// Compile a single column clause
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public virtual void VisitColumn(AbstractColumn column)
    {
        ArgumentNullException.ThrowIfNull(column);

        switch (column)
        {
            case RawColumn raw:
                AddParameters(raw.Bindings);
                SqlBuilder.Append(raw.Expression);
                break;
            case QueryColumn queryColumn:
                SqlBuilder.Append('(');

                VisitQuery(queryColumn.Query);

                SqlBuilder.Append(')');

                if (!string.IsNullOrEmpty(queryColumn.Query.QueryAlias))
                {
                    SqlBuilder.Append(' ')
                        .Append(ColumnAsKeyword)
                        .Append(WrapValue(queryColumn.Query.QueryAlias));
                }
                break;
            case AggregatedColumn aggregatedColumn:
                {
                    var (col, alias) = SplitAlias(aggregatedColumn.Column.Name);

                    SqlBuilder.Append(aggregatedColumn.Aggregate.ToUpperInvariant())
                        .Append('(')
                        .Append(Wrap(col))
                        .Append(')');

                    if (!string.IsNullOrEmpty(alias))
                    {
                        SqlBuilder
                            .Append(' ')
                            .Append(ColumnAsKeyword)
                            .Append(alias);
                    }
                    //var filterCondition = CompileFilterConditions(aggregatedColumn);

                    //if (string.IsNullOrEmpty(filterCondition))
                    //{
                    //    return $"{agg}({col}){alias}";
                    //}

                    //if (SupportsFilterClause)
                    //{
                    //    return $"{agg}({col}) FILTER (WHERE {filterCondition}){alias}";
                    //}
                }
                break;
            default:
                SqlBuilder.Append(Wrap(((Column)column).Name));
                break;
        }
    }

    protected virtual void VisitFilterConditions(Query query, AggregatedColumn aggregatedColumn)
    {
        if (aggregatedColumn.Filter == null)
        {
            return;
        }

        var wheres = aggregatedColumn.Filter.GetComponents<AbstractCondition>(Component.Where);

        VisitConditions(query, wheres);
    }

    //public virtual SqlResult CompileCte(AbstractFrom cte)
    //{
    //    var ctx = new SqlResult();

    //    if (null == cte)
    //    {
    //        return ctx;
    //    }

    //    //if (cte is RawFromClause raw)
    //    //{
    //    //    Bindings.AddRange(raw.Bindings);
    //    //    RawSql = $"{WrapValue(raw.Alias!)} AS ({raw.Expression})";
    //    //}
    //    //else if (cte is QueryFromClause queryFromClause)
    //    //{
    //    //    var subCtx = CompileSelectQuery(queryFromClause.Query);
    //    //    Bindings.AddRange(subBindings);

    //    //    RawSql = $"{WrapValue(queryFromClause.Alias!)} AS ({subRawSql})";
    //    //}
    //    //else if (cte is AdHocTableFromClause adHoc)
    //    //{
    //    //    var subCtx = CompileAdHocQuery(adHoc);
    //    //    Bindings.AddRange(subBindings);

    //    //    RawSql = $"{WrapValue(adHoc.Alias!)} AS ({subRawSql})";
    //    //}

    //    return ctx;
    //}

    protected virtual void VisitProjection(Query query)
    {
        SqlBuilder.Append("SELECT ");

        if (query.IsDistinct)
        {
            SqlBuilder.Append("DISTINCT ");
        }

        if (query.HasComponent(Component.Aggregate))
        {
            var aggregate = query.GetOneComponent<AggregateClause>(Component.Aggregate)!;

            if (aggregate.Columns.Count == 1)
            {
                SqlBuilder.Append(aggregate.Type.ToUpperInvariant())
                    .Append('(');

                VisitColumn(new Column { Name = aggregate.Columns[0] });

                SqlBuilder.Append(") ")
                    .Append(ColumnAsKeyword)
                    .Append(WrapValue(aggregate.Type));
            }

            SqlBuilder.Append('1');

            return;
        }

        var columns = query.GetComponents<AbstractColumn>(Component.Select);

        if (columns.Count > 0)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                if (i != 0)
                {
                    SqlBuilder.Append(", ");
                }

                VisitColumn(columns[i]);
            }
        }
        else
        {
            SqlBuilder.Append('*');
        }
    }

    public virtual void VisitUnion(Query query)
    {
        var clauses = query.GetComponents<AbstractCombine>(Component.Combine);

        foreach (var clause in clauses)
        {
            switch (clause)
            {
                case Combine combineClause:
                    SqlBuilder.Append(' ')
                        .Append(combineClause.Operation.ToUpperInvariant())
                        .Append(combineClause.All ? " ALL " : " ");

                    VisitQuery(combineClause.Query);
                    break;
                case RawCombine rawCombine:
                    AddParameters(rawCombine.Bindings);
                    SqlBuilder.Append(' ').Append(rawCombine.Expression);
                    break;
                default:
                    throw new InvalidOperationException("TODO: ");
            }
        }
    }

    public virtual void VisitTableExpression(AbstractFrom from)
    {
        switch (from)
        {
            case RawFromClause raw:
                AddParameters(raw.Bindings);
                SqlBuilder.Append(raw.Expression);
                break;
            case QueryFromClause queryFromClause:
                var fromQuery = queryFromClause.Query;

                SqlBuilder.Append('(');

                VisitQuery(fromQuery);

                SqlBuilder.Append(')');

                if (!string.IsNullOrEmpty(fromQuery.QueryAlias))
                {
                    SqlBuilder.Append(TableAsKeyword)
                        .Append(WrapValue(fromQuery.QueryAlias));
                }
                break;
            case FromClause fromClause:
                SqlBuilder.Append(Wrap(fromClause.Table));
                break;
            default:
                throw new InvalidCastException(
                    $"Invalid type \"{from.GetType().Name}\" provided for the 'TableExpression' clause.");
        }
    }

    public virtual void VisitFrom(Query query)
    {
        if (!query.HasComponent(Component.From))
        {
            return;
        }

        var from = query.GetOneComponent<AbstractFrom>(Component.From)
            ?? throw new InvalidOperationException("TODO: ");

        SqlBuilder.Append(" FROM ");

        VisitTableExpression(from);
    }

    public virtual void VisitJoins(Query query)
    {
        if (!query.HasComponent(Component.Join))
        {
            return;
        }

        var joins = query.GetComponents<BaseJoin>(Component.Join);

        for (var i = 0; i < joins.Count; i++)
        {
            VisitJoin(query, joins[i].Join);
        }
    }

    public virtual void VisitJoin(Query query, Join join, bool isNested = false)
    {
        var from = join.GetOneComponent<AbstractFrom>(Component.From)!;
        var conditions = join.GetComponents<AbstractCondition>(Component.Where);

        if (conditions.Count == 0)
        {
            return;
        }

        SqlBuilder
            .Append(' ')
            .Append(join.Type)
            .Append(' ');

        VisitTableExpression(from);

        SqlBuilder
            .Append(" ON ");

        VisitConditions(query, conditions);
    }

    public virtual void VisitWheres(Query query)
    {
        var conditions = query.GetComponents<AbstractCondition>(Component.Where);

        if (conditions.Count == 0)
        {
            return;
        }

        SqlBuilder.Append(" WHERE ");

        VisitConditions(query, conditions);
    }

    public virtual void VisitGroups(Query query)
    {
        var columns = query.GetComponents<AbstractColumn>(Component.Group);

        if (columns.Count == 0)
        {
            return;
        }

        SqlBuilder.Append("GROUP BY ");

        for (var i = 0; i < columns.Count; i++)
        {
            if (i != 0)
            {
                SqlBuilder.Append(", ");
            }

            SqlBuilder.Append(columns[i]);
        }
    }

    public virtual void VisitOrders(Query query)
    {
        var orders = query.GetComponents<AbstractOrderBy>(Component.Order);

        if (orders.Count == 0)
        {
            return;
        }

        SqlBuilder.Append(" ORDER BY ");

        for (var i = 0; i < orders.Count; i++)
        {
            if (i != 0)
            {
                SqlBuilder.Append(", ");
            }

            VisitOrder(orders[i]);
        }
    }

    private void VisitOrder(AbstractOrderBy orderBy)
    {
        switch (orderBy)
        {
            case RawOrderBy raw:
                SqlBuilder.Append(raw.Expression);
                break;
            case OrderBy order:
                SqlBuilder.Append(Wrap(order.Column));
                if (!order.Ascending)
                {
                    SqlBuilder.Append(" DESC");
                }
                break;
            default: break;
        }
    }

    public virtual string? VisitHaving(Query query)
    {
        var sql = new List<string>();
        string boolOperator;

        var having = query.GetComponents<AbstractCondition>(Component.Having)
            .Cast<AbstractCondition>()
            .ToList();

        for (var i = 0; i < having.Count; i++)
        {
            VisitCondition(query, having[i]);

            boolOperator = i > 0 ? having[i].IsOr ? "OR " : "AND " : "";

            //sql.Add(boolOperator + compiled);
        }

        return $"HAVING {string.Join(" ", sql)}";
    }

    public virtual void VisitLimit(Query query)
    {
        var limit = query.GetLimit();
        var offset = query.GetOffset();

        if (limit == 0 && offset == 0)
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
            SqlBuilder.Append(" LIMIT 18446744073709551615 OFFSET ")
                .Append(Parameter(query, offset));

            return;
        }

        SqlBuilder.Append(" LIMIT ")
            .Append(Parameter(query, limit))
            .Append(" OFFSET ")
            .Append(Parameter(query, offset));
    }

    protected virtual string CompileTrue()
        => "true";

    protected virtual string CompileFalse()
        => "false";

    protected static string CheckOperator(string operation)
        => Operators.Contains(operation.ToLowerInvariant())
        ? operation
        : throw new InvalidOperationException($"The operator '{operation}' cannot be used. Please consider white listing it before using it.");

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

    protected static (string Column, string? Alias) SplitAlias(string value)
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
    protected virtual object Resolve(Query query, object parameter)
        => parameter switch
        {
            UnsafeLiteral literal => literal.Value,
            Variable variable => query.FindVariable(variable.Name),
            _ => parameter
        };

    /// <summary>
    /// Resolve a parameter and add it to the binding list
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    protected virtual string Parameter(Query query, object parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        switch (parameter)
        {
            case UnsafeLiteral literal:
                return literal.Value;
            case Variable variable:
                {
                    var paramName = GetParamName();
                    var value = query.FindVariable(variable.Name);
                    NamedBindings.Add(paramName, value);
                    return paramName;
                }
            default:
                {
                    var paramName = GetParamName();
                    NamedBindings.Add(paramName, parameter);
                    return paramName;
                }
        }
    }

    /// <summary>
    /// Create query parameter place-holders for an array.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    protected virtual string Parameterize<T>(Query query, IEnumerable<T> values)
        where T : notnull
        => string.Join(", ", values.Select(x => Parameter(query, x)));

    protected virtual void VisitCondition(Query query, AbstractCondition clause)
    {
        switch (clause)
        {
            case BasicStringCondition basicStringCondition:
                VisitBasicStringCondition(query, basicStringCondition);
                break;
            case BasicDateCondition basicDateCondition:
                VisitBasicDateCondition(query, basicDateCondition);
                break;
            case BasicCondition basicCondition:
                VisitBasicCondition(query, basicCondition);
                break;
            case TwoColumnsCondition twoColumnsCondition:
                VisitTwoColumnsCondition(twoColumnsCondition);
                break;
            case BooleanCondition booleanCondition:
                VisitBooleanCondition(booleanCondition);
                break;
            case NullCondition nullCondition:
                VisitNullCondition(nullCondition);
                break;
            case ExistsCondition existsCondition:
                VisitExistsCondition(existsCondition);
                break;
            case InCondition<int> inCondition:
                VisitInCondition(query, inCondition);
                break;
            case BetweenCondition<DateTime> betweenCondition:
                VisitBetweenCondition(query, betweenCondition);
                break;
            case QueryCondition<Query> queryCondition:
                VisitQueryCondition(queryCondition);
                break;
            case SubQueryCondition<Query> subQueryCondition:
                VisitSubQueryCondition(query, subQueryCondition);
                break;
            case InQueryCondition inQueryCondition:
                VisitInQueryCondition(inQueryCondition);
                break;
            default:
                var clauseType = clause.GetType();

                var methodName = clauseType switch
                {
                    _ when clauseType.IsGenericType && clauseType.GetGenericTypeDefinition() == typeof(InCondition<>) => nameof(VisitInCondition),
                    _ when clauseType.IsGenericType && clauseType.GetGenericTypeDefinition() == typeof(BetweenCondition<>) => nameof(VisitBetweenCondition),
                    _ when clauseType.IsGenericType && clauseType.GetGenericTypeDefinition() == typeof(NestedCondition<>) => nameof(VisitNestedCondition),
                    _ => throw new InvalidCastException(clauseType.FullName),
                };

                var genericArgunement = clause.GetType().GenericTypeArguments[0];
                var cacheKey = $"{methodName}:{genericArgunement.Name}"["Compile".Length..];

                if (!_methods.TryGetValue(cacheKey, out var methodInfo))
                {
                    methodInfo = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
                        .MakeGenericMethod(genericArgunement);

                    _methods.Add(cacheKey, methodInfo);
                }

                methodInfo.Invoke(this, new object[] { query, clause });
                break;
        };
    }

    protected virtual void VisitConditions(Query query, List<AbstractCondition> conditions)
    {
        for (var i = 0; i < conditions.Count; i++)
        {
            if (i != 0)
            {
                SqlBuilder.Append(conditions[i].IsOr ? " OR " : " AND ");
            }

            VisitCondition(query, conditions[i]);
        }
    }

    protected virtual void VisitQueryCondition<T>(QueryCondition<T> x) where T : BaseQuery<T>
    {
        SqlBuilder.Append(Wrap(x.Column))
            .Append(' ')
            .Append(CheckOperator(x.Operator))
            .Append(" (");

        VisitQuery(x.Query);

        SqlBuilder.Append(')');
    }

    protected virtual void VisitSubQueryCondition<T>(Query query, SubQueryCondition<T> x) where T : BaseQuery<T>
    {
        SqlBuilder.Append('(');

        VisitQuery(x.Query);

        SqlBuilder.Append(") ")
            .Append(CheckOperator(x.Operator))
            .Append(' ')
            .Append(Parameter(query, x.Value));
    }

    protected virtual void VisitBasicCondition(Query query, BasicCondition x)
    {
        if (x.IsNot)
        {
            SqlBuilder.Append("NOT ");
        }

        SqlBuilder.Append(Wrap(x.Column))
            .Append(' ')
            .Append(CheckOperator(x.Operator))
            .Append(' ')
            .Append(Parameter(query, x.Value));
    }

    protected virtual void VisitBasicStringCondition(Query query, BasicStringCondition x)
    {
        if (Resolve(query, x.Value) is not string value)
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
            SqlBuilder.Append("NOT (");
        }

        SqlBuilder.Append(Wrap(x.Column))
            .Append(' ')
            .Append(CheckOperator(method))
            .Append(' ')
            .Append(x.Value is UnsafeLiteral ? value : Parameter(query, value));

        if (!string.IsNullOrEmpty(x.EscapeCharacter))
        {
            SqlBuilder.Append(" ESCAPE ")
                .Append('\'')
                .Append(x.EscapeCharacter)
                .Append('\'');
        }

        if (x.IsNot)
        {
            SqlBuilder.Append(") ");
        }
    }

    protected virtual void VisitBasicDateCondition(Query query, BasicDateCondition x)
    {
        var column = Wrap(x.Column);
        var op = CheckOperator(x.Operator);

        if (x.IsNot)
        {
            SqlBuilder.Append("NOT (");
        }

        SqlBuilder.Append(x.Part.ToUpperInvariant())
            .Append('(')
            .Append(column)
            .Append(") ")
            .Append(op)
            .Append(' ')
            .Append(Parameter(query, x.Value));

        if (x.IsNot)
        {
            SqlBuilder.Append(") ");
        }
    }

    protected virtual void VisitNestedCondition<Q>(Query query, NestedCondition<Q> x) where Q : BaseQuery<Q>
    {
        if (!(x.Query.HasComponent(Component.Where) || x.Query.HasComponent(Component.Having)))
        {
            return;
        }

        var clause = x.Query.HasComponent(Component.Where) ? Component.Where : Component.Having;

        var clauses = x.Query.GetComponents<AbstractCondition>(clause);

        if (x.IsNot)
        {
            SqlBuilder.Append("NOT ");
        }

        SqlBuilder.Append('(');

        VisitConditions(query, clauses);

        SqlBuilder.Append(')');
    }

    protected void VisitTwoColumnsCondition(TwoColumnsCondition clause)
    {
        ArgumentNullException.ThrowIfNull(clause);

        if (clause.IsNot)
        {
            SqlBuilder.Append("NOT ");
        }

        SqlBuilder.Append(Wrap(clause.First))
            .Append(' ')
            .Append(CheckOperator(clause.Operator))
            .Append(' ')
            .Append(Wrap(clause.Second));
    }

    protected virtual void VisitBetweenCondition<T>(Query query, BetweenCondition<T> item)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(item);

        var lower = Parameter(query, item.Lower!);
        var higher = Parameter(query, item.Higher!);

        SqlBuilder.Append(Wrap(item.Column))
            .Append(item.IsNot ? " NOT BETWEEN " : " BETWEEN ")
            .Append(lower)
            .Append(" AND ")
            .Append(higher);
    }

    protected virtual void VisitInCondition<T>(Query query, InCondition<T> item)
        where T : notnull
    {
        if (!item.Values.Any())
        {
            SqlBuilder.Append(item.IsNot ? "1 = 1 /* NOT IN [empty list] */" : "1 = 0 /* IN [empty list] */");
            return;
        }

        SqlBuilder.Append(Wrap(item.Column))
            .Append(item.IsNot ? " NOT IN " : " IN ")
            .Append('(')
            .Append(Parameterize(query, item.Values))
            .Append(')');
    }

    protected virtual void VisitInQueryCondition(InQueryCondition item)
    {
        SqlBuilder.Append(Wrap(item.Column))
            .Append(item.IsNot ? " NOT IN (" : " IN (");

        VisitQuery(item.Query);

        SqlBuilder.Append(')');
    }

    protected virtual void VisitNullCondition(NullCondition item)
        => SqlBuilder.Append(Wrap(item.Column))
        .Append(item.IsNot ? " IS NOT NULL" : " IS NULL");

    protected virtual void VisitBooleanCondition(BooleanCondition item)
        => SqlBuilder.Append(Wrap(item.Column))
        .Append(item.IsNot ? " != " : " = ")
        .Append(item.Value ? CompileTrue() : CompileFalse());

    protected virtual void VisitExistsCondition(ExistsCondition item)
    {
        SqlBuilder.Append(item.IsNot ? "NOT EXISTS " : "EXISTS ");

        // remove unneeded components
        var query = item.Query.Clone();

        if (OmitSelectInsideExists)
        {
            query.ClearComponent(Component.Select).SelectRaw("1");
        }

        SqlBuilder.Append('(');

        VisitQuery(query);

        SqlBuilder.Append(')');
    }
}
