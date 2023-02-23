namespace Argon.QueryBuilder.Clauses;

public abstract class AbstractColumn : AbstractClause
{
}

/// <summary>
/// Represents "column" or "column as alias" clause.
/// </summary>
/// <seealso cref="AbstractColumn" />
public class Column : AbstractColumn
{
    /// <summary>
    /// Gets or sets the column name. Can be "columnName" or "columnName as columnAlias".
    /// </summary>
    /// <value>
    /// The column name.
    /// </value>
    public required string Name { get; set; }
    public string? Table { get; set; }
    public string? Alias { get; set; }
    /// <inheritdoc />
    public override AbstractClause Clone()
        => new Column
        {
            Name = Name,
            Alias = Alias,
        };
}

/// <summary>
/// Represents column clause calculated using query.
/// </summary>
/// <seealso cref="AbstractColumn" />
public class QueryColumn : AbstractColumn
{
    /// <summary>
    /// Gets or sets the query that will be used for column value calculation.
    /// </summary>
    /// <value>
    /// The query for column value calculation.
    /// </value>
    public required Query Query { get; set; }
    public override AbstractClause Clone()
        => new QueryColumn
        {
            Query = Query.Clone(),
        };
}

public class RawColumn : AbstractColumn
{
    /// <summary>
    /// Gets or sets the RAW expression.
    /// </summary>
    /// <value>
    /// The RAW expression.
    /// </value>
    public required string Expression { get; set; }
    public required object[] Bindings { set; get; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new RawColumn
        {
            Expression = Expression,
            Bindings = Bindings,
        };
}

/// <summary>
/// Represents an aggregated column clause with an optional filter
/// </summary>
/// <seealso cref="AbstractColumn" />
public class AggregatedColumn : AbstractColumn
{
    /// <summary>
    /// Gets or sets the a query that used to filter the data, 
    /// the compiler will consider only the `Where` clause.
    /// </summary>
    /// <value>
    /// The filter query.
    /// </value>
    public Query? Filter { get; set; }
    public required string Aggregate { get; set; }
    public required Column Column { get; set; }
    public override AbstractClause Clone()
        => new AggregatedColumn
        {
            Filter = Filter?.Clone(),
            Column = (Column)Column.Clone(),
            Aggregate = Aggregate,
        };
}
