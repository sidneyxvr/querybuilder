namespace Argon.QueryBuilder.Clauses;

public abstract class AbstractClause
{
    /// <summary>
    /// Gets or sets the component name.
    /// </summary>
    /// <value>
    /// The component name.
    /// </value>
    public Component? Component { get; set; }
    public abstract AbstractClause Clone();
}
