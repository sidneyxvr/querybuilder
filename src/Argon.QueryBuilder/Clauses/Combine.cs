namespace Argon.QueryBuilder.Clauses;

public abstract class AbstractCombine : AbstractClause
{

}

public class Combine : AbstractCombine
{
    /// <summary>
    /// Gets or sets the query to be combined with.
    /// </summary>
    /// <value>
    /// The query that will be combined.
    /// </value>
    public required Query Query { get; set; }

    /// <summary>
    /// Gets or sets the combine operation, e.g. "UNION", etc.
    /// </summary>
    /// <value>
    /// The combine operation.
    /// </value>
    public required string Operation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Combine"/> clause will combine all.
    /// </summary>
    /// <value>
    ///   <c>true</c> if all; otherwise, <c>false</c>.
    /// </value>
    public bool All { get; set; }
}
