using Argon.QueryBuilder.Clauses;
using System.Reflection;
using System.Text;

namespace Argon.QueryBuilder;

public interface IQuerySqlGenerator
{
    abstract static SqlResult Compile(Query query);
}

public abstract class QuerySqlGenerator
{
    private static readonly Dictionary<string, MethodInfo> _methods = new();

    protected readonly StringBuilder SqlBuilder = new();

    protected readonly Dictionary<string, object> NamedBindings = new();

    private int index = 0;

    private string GetParamName()
        => $"@p{index++}";

    public void AddParameter(object value)
        => NamedBindings.Add(GetParamName(), value);

    public void AddParameters(object[] value)
    {
        foreach (var item in value)
        {
            AddParameter(item);
        }
    }

    protected static string OpeningIdentifier { get; set; } = "\"";
    protected static string ClosingIdentifier { get; set; } = "\"";
    protected static string ColumnAsKeyword { get; set; } = " AS ";
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
        var aggregate = query.AggregateColumns[0];

        if (aggregate.Columns.Count == 1 && !query.IsDistinct) return query;

        if (query.IsDistinct)
        {
            query.AggregateColumns.Clear();
            query.Columns.Clear();
            foreach (var column in aggregate.Columns)
            {
                query.AddComponent(ComponentType.Select, column);
            }
        }
        else
        {
            foreach (var column in aggregate.Columns)
            {
                query.WhereNotNull(column.Alias is null ? column.Name : $"{column.Name}.{column.Alias}");
            }
        }

        var outerClause = new AggregateClause()
        {
            Columns = new List<Column>(1) { new Column { Name = "*" } },
            Type = aggregate.Type
        };

        return new Query()
            .AddComponent(ComponentType.Aggregate, outerClause)
            .From(query, $"{aggregate.Type}Query");
    }

    protected virtual SqlResult CompileQuery(Query query)
    {
        if (query.Method == MethodType.Aggregate)
        {
            query.LimitClause = null;
            query.OrderByColumns.Clear();
            query.GroupByColumns.Clear();

            query = TransformAggregateQuery(query);
        }

        VisitSelect(query);

        // handle CTEs
        //if (query.HasComponent(Component.Cte))
        //{
        //    //CompileCteQuery(query);
        //}

        return new SqlResult
        {
            Sql = SqlBuilder.ToString(),
            Parameters = NamedBindings.AsReadOnly(),
        };
    }

    protected virtual void VisitSelect(Query query)
    {
        SqlBuilder.Append("SELECT ");

        if (query.IsDistinct)
        {
            SqlBuilder.Append("DISTINCT ");
        }

        if (query.AggregateColumns.Count > 0)
        {
            VisitAggregate(query.AggregateColumns[0]);
        }
        else
        {
            VisitProjection(query.Columns);
        }

        if (query.FromClause is null)
        {
            throw new InvalidOperationException("No FROM clause found");
        }

        SqlBuilder.Append(" FROM ");

        VisitFrom(query.FromClause);

        if (query.Joins.Count > 0)
        {
            VisitJoins(query, query.Joins);
        }

        if (query.Conditions.Count > 0)
        {
            SqlBuilder.Append(" WHERE ");

            VisitConditions(query.Conditions);
        }

        if (query.GroupByColumns.Count > 0)
        {
            SqlBuilder.Append(" GROUP BY ");

            VisitGroups(query.GroupByColumns);
        }

        if (query.HavingClause is not null)
        {
            SqlBuilder.Append(" HAVING ");

            VisitHaving(query.HavingClause);
        }

        if (query.OrderByColumns.Count > 0)
        {
            SqlBuilder.Append(" ORDER BY ");

            VisitOrders(query.OrderByColumns);
        }

        VisitLimitOffset(query.LimitClause, query.OffsetClause);

        if (query.Unions.Count > 0)
        {
            VisitUnions(query.Unions);
        }
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
    public virtual void VisitColumn(AbstractColumn columnClause)
    {
        ArgumentNullException.ThrowIfNull(columnClause);

        switch (columnClause)
        {
            case ConstColumn constant:
                SqlBuilder.Append(constant.Value);
                break;
            case QueryColumn queryColumn:
                SqlBuilder.Append('(');

                VisitSelect(queryColumn.Query);

                SqlBuilder.Append(')');

                if (!string.IsNullOrEmpty(queryColumn.Query.QueryAlias))
                {
                    SqlBuilder.Append(ColumnAsKeyword)
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
            case Column column:
                if (column.Schema is not null)
                {
                    SqlBuilder.Append(WrapValue(column.Schema))
                        .Append('.');
                }

                SqlBuilder.Append(Wrap(column.Name));
                if (column.Alias is not null)
                {
                    SqlBuilder.Append(ColumnAsKeyword)
                        .Append(column.Alias);
                }

                break;
        }
    }

    protected virtual void VisitFilterConditions(Query query, AggregatedColumn aggregatedColumn)
    {
        if (aggregatedColumn.Filter == null)
        {
            return;
        }

        var wheres = aggregatedColumn.Filter.Conditions;

        VisitConditions(wheres);
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

    protected virtual void ProjectionPreProcessor(Query query)
    {
        if (query.AggregateColumns.Count == 0)
        {
            return;
        }

        var aggregate = query.AggregateColumns[0];

        if (aggregate.Columns.Count == 1)
        {
            SqlBuilder.Append(aggregate.Type.ToUpperInvariant())
                .Append('(');

            VisitColumn(aggregate.Columns[0]);

            SqlBuilder.Append(')')
                .Append(ColumnAsKeyword)
                .Append(WrapValue(aggregate.Type));
        }

        SqlBuilder.Append('1');
    }

    protected virtual void VisitAggregate(AggregateClause aggregate)
    {
        if (aggregate.Columns.Count == 1)
        {
            SqlBuilder.Append(aggregate.Type.ToUpperInvariant())
                .Append('(');

            VisitColumn(aggregate.Columns[0]);

            SqlBuilder.Append(')')
                .Append(ColumnAsKeyword)
                .Append(WrapValue(aggregate.Type));

            return;
        }

        SqlBuilder.Append('1');
    }

    protected virtual void VisitProjection(IReadOnlyList<AbstractColumn> columns)
    {
        if (columns.Count > 0)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                if (i > 0)
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

    public virtual void VisitUnions(IReadOnlyList<AbstractCombine> combines)
    {
        foreach (var combine in combines)
        {
            switch (combine)
            {
                case Combine combineClause:
                    SqlBuilder.Append(' ')
                        .Append(combineClause.Operation.ToUpperInvariant())
                        .Append(combineClause.All ? " ALL " : " ");

                    VisitSelect(combineClause.Query);
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
            case QueryFromClause queryFromClause:
                var fromQuery = queryFromClause.Query;

                SqlBuilder.Append('(');

                VisitSelect(fromQuery);

                SqlBuilder.Append(')');

                if (!string.IsNullOrEmpty(fromQuery.QueryAlias))
                {
                    SqlBuilder.Append(TableAsKeyword)
                        .Append(WrapValue(fromQuery.QueryAlias));
                }
                break;
            case FromClause fromClause:
                SqlBuilder.Append(WrapValue(fromClause.Table));

                if (!string.IsNullOrEmpty(fromClause.Alias))
                {
                    SqlBuilder.Append(TableAsKeyword)
                        .Append(WrapValue(fromClause.Alias));
                }
                break;
            default:
                throw new InvalidCastException(
                    $"Invalid type \"{from.GetType().Name}\" provided for the 'TableExpression' clause.");
        }
    }

    public virtual void VisitFrom(AbstractFrom fromClause)
        => VisitTableExpression(fromClause);

    public virtual void VisitJoins(Query query, IReadOnlyList<BaseJoin> joins)
    {
        for (var i = 0; i < joins.Count; i++)
        {
            VisitJoin(query, joins[i].Join);
        }
    }

    public virtual void VisitJoin(Query query, Join join, bool isNested = false)
    {
        var from = join.FromClause!;
        var conditions = join.Conditions;

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

        VisitConditions(conditions);
    }

    public virtual void VisitGroups(IReadOnlyList<AbstractColumn> groups)
    {
        for (var i = 0; i < groups.Count; i++)
        {
            if (i > 0)
            {
                SqlBuilder.Append(", ");
            }

            SqlBuilder.Append(groups[i]);
        }
    }

    public virtual void VisitOrders(IReadOnlyList<AbstractOrderBy> orders)
    {
        for (var i = 0; i < orders.Count; i++)
        {
            if (i > 0)
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

    public virtual void VisitHaving(AbstractCondition having)
    {
        VisitCondition(having);

        // boolOperator = i > 0 ? having[i].IsOr ? "OR " : "AND " : "";

        //sql.Add(boolOperator + compiled);
        // return $"HAVING {string.Join(" ", sql)}";
    }

    public virtual void VisitLimitOffset(LimitClause? limitClause, OffsetClause? offsetClause)
    {
        var limit = limitClause?.Limit ?? 0;
        var offset = offsetClause?.Offset ?? 0;

        if (limit == 0 && offset == 0)
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
            SqlBuilder.Append(" LIMIT 18446744073709551615 OFFSET ")
                .Append(Parameter(offset));

            return;
        }

        SqlBuilder.Append(" LIMIT ")
            .Append(Parameter(limit))
            .Append(" OFFSET ")
            .Append(Parameter(offset));
    }

    protected virtual string DbValueTrue()
        => "true";

    protected virtual string DbValueFalse()
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
        if (value.Contains('.'))
        {
            var splitedValue = value.Split('.');

            return $"{WrapValue(splitedValue[0])}.{WrapValue(splitedValue[1])}";
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
        => value == "*"
        ? value
        : $"{OpeningIdentifier}{value}{ClosingIdentifier}";

    /// <summary>
    /// Resolve a parameter and add it to the binding list
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    protected virtual string Parameter(object parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        switch (parameter)
        {
            case UnsafeLiteral literal:
                return literal.Value;
            case Variable variable:
                {
                    var paramName = GetParamName();
                    //var value = query.FindVariable(variable.Name);
                    //NamedBindings.Add(paramName, value);
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
    protected virtual string Parameterize<T>(IEnumerable<T> values)
        where T : notnull
        => string.Join(", ", values.Select(x => Parameter(x)));

    protected virtual void VisitCondition(AbstractCondition clause)
    {
        switch (clause)
        {
            case BasicStringCondition basicStringCondition:
                VisitBasicStringCondition(basicStringCondition);
                break;
            case BasicDateCondition basicDateCondition:
                VisitBasicDateCondition(basicDateCondition);
                break;
            case BasicCondition basicCondition:
                VisitBasicCondition(basicCondition);
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
                VisitInCondition(inCondition);
                break;
            case BetweenCondition<DateTime> betweenCondition:
                VisitBetweenCondition(betweenCondition);
                break;
            case QueryCondition<Query> queryCondition:
                VisitQueryCondition(queryCondition);
                break;
            case SubQueryCondition<Query> subQueryCondition:
                VisitSubQueryCondition(subQueryCondition);
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

                methodInfo.Invoke(this, new object[] { clause });
                break;
        };
    }

    protected virtual void VisitConditions(IReadOnlyList<AbstractCondition> conditions)
    {
        for (var i = 0; i < conditions.Count; i++)
        {
            if (i > 0)
            {
                SqlBuilder.Append(conditions[i].IsOr ? " OR " : " AND ");
            }

            VisitCondition(conditions[i]);
        }
    }

    protected virtual void VisitQueryCondition<T>(QueryCondition<T> x) where T : BaseQuery<T>
    {
        SqlBuilder.Append(Wrap(x.Column))
            .Append(' ')
            .Append(CheckOperator(x.Operator))
            .Append(" (");

        VisitSelect(x.Query);

        SqlBuilder.Append(')');
    }

    protected virtual void VisitSubQueryCondition<T>(SubQueryCondition<T> x) where T : BaseQuery<T>
    {
        SqlBuilder.Append('(');

        VisitSelect(x.Query);

        SqlBuilder.Append(") ")
            .Append(CheckOperator(x.Operator))
            .Append(' ')
            .Append(Parameter(x.Value));
    }

    protected virtual void VisitBasicCondition(BasicCondition x)
    {
        if (x.IsNot)
        {
            SqlBuilder.Append("NOT ");
        }

        SqlBuilder.Append(Wrap(x.Column))
            .Append(' ')
            .Append(CheckOperator(x.Operator))
            .Append(' ');

        if (x.Value is Query query)
        {
            SqlBuilder.Append('(');

            VisitSelect(query);

            SqlBuilder.Append(')');
        }
        else
        {
            SqlBuilder.Append(Parameter(x.Value));
        }
    }

    protected virtual void VisitBasicStringCondition(BasicStringCondition x)
    {
        var value = x.Value;
        var method = x.Operator;

        if (new[] { "starts", "ends", "contains", "like" }.Contains(x.Operator))
        {
            method = "LIKE";

            value = x.Operator switch
            {
                "starts" => $"{x.Value}%",
                "ends" => $"%{x.Value}",
                "contains" => $"%{x.Value}%",
                _ => x.Value
            };
        }

        if (x.IsNot)
        {
            SqlBuilder.Append("NOT (");
        }

        SqlBuilder.Append(Wrap(x.Column))
            .Append(' ')
            .Append(CheckOperator(method))
            .Append(' ')
            .Append(x.Value is UnsafeLiteral ? value : Parameter(value));

        if (!string.IsNullOrEmpty(x.EscapeCharacter))
        {
            SqlBuilder.Append(" ESCAPE ")
                .Append('\'')
                .Append(x.EscapeCharacter)
                .Append('\'');
        }

        if (x.IsNot)
        {
            SqlBuilder.Append(')');
        }
    }

    protected virtual void VisitBasicDateCondition(BasicDateCondition x)
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
            .Append(Parameter(x.Value));

        if (x.IsNot)
        {
            SqlBuilder.Append(')');
        }
    }

    protected virtual void VisitNestedCondition<Q>(NestedCondition<Q> x) where Q : BaseQuery<Q>
    {
        if (x.Query is { Conditions.Count: 0, HavingClause: null })
        {
            return;
        }

        var clauses = x.Query.Conditions.Count > 0
            ? x.Query.Conditions
            : new List<AbstractCondition>(1) { x.Query.HavingClause! };

        if (x.IsNot)
        {
            SqlBuilder.Append("NOT ");
        }

        SqlBuilder.Append('(');

        VisitConditions(clauses);

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

    protected virtual void VisitBetweenCondition<T>(BetweenCondition<T> item)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(item);

        var lower = Parameter(item.Lower!);
        var higher = Parameter(item.Higher!);

        SqlBuilder.Append(WrapValue(item.Column))
            .Append(item.IsNot ? " NOT BETWEEN " : " BETWEEN ")
            .Append(lower)
            .Append(" AND ")
            .Append(higher);
    }

    protected virtual void VisitInCondition<T>(InCondition<T> item)
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
            .Append(Parameterize(item.Values))
            .Append(')');
    }

    protected virtual void VisitInQueryCondition(InQueryCondition item)
    {
        SqlBuilder.Append(Wrap(item.Column))
            .Append(item.IsNot ? " NOT IN (" : " IN (");

        VisitSelect(item.Query);

        SqlBuilder.Append(')');
    }

    protected virtual void VisitNullCondition(NullCondition item)
        => SqlBuilder.Append(Wrap(item.Column))
        .Append(item.IsNot ? " IS NOT NULL" : " IS NULL");

    protected virtual void VisitBooleanCondition(BooleanCondition item)
        => SqlBuilder.Append(Wrap(item.Column))
        .Append(item.IsNot ? " != " : " = ")
        .Append(item.Value ? DbValueTrue() : DbValueFalse());

    protected virtual void VisitExistsCondition(ExistsCondition item)
    {
        SqlBuilder.Append(item.IsNot ? "NOT EXISTS " : "EXISTS ");

        var query = item.Query.Clone();

        if (OmitSelectInsideExists)
        {
            query.Columns.Clear();
            query.SelectConstant(1);
        }

        SqlBuilder.Append('(');

        VisitSelect(query);

        SqlBuilder.Append(')');
    }
}
