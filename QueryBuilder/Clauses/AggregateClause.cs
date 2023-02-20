namespace QueryBuilder.Clauses;

/// <summary>
/// Represents aggregate clause like "COUNT", "MAX" or etc.
/// </summary>
/// <seealso cref="AbstractClause" />
public class AggregateClause : AbstractClause
{
    /// <summary>
    /// Gets or sets columns that used in aggregate clause.
    /// </summary>
    /// <value>
    /// The columns to be aggregated.
    /// </value>
    public required List<string> Columns { get; set; }

    /// <summary>
    /// Gets or sets the type of aggregate function.
    /// </summary>
    /// <value>
    /// The type of aggregate function, e.g. "MAX", "MIN", etc.
    /// </value>
    public required string Type { get; set; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new AggregateClause
        {
            Engine = Engine,
            Type = Type,
            Columns = new List<string>(Columns),
            Component = Component,
        };
}
