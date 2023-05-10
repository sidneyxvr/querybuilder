namespace Argon.QueryBuilder.Clauses;

public abstract class AbstractFrom : AbstractClause
{
    public string? Alias { get; set; }
}

/// <summary>
/// Represents a "from" clause.
/// </summary>
public class FromClause : AbstractFrom
{
    public required string Table { get; set; }
}

/// <summary>
/// Represents a "from subquery" clause.
/// </summary>
public class QueryFromClause : AbstractFrom
{
    public required Query Query { get; set; }
}

/// <summary>
/// Represents a FROM clause that is an ad-hoc table built with predefined values.
/// </summary>
public class AdHocTableFromClause : AbstractFrom
{
    public required List<string> Columns { get; set; }
    public required List<object> Values { get; set; }
}
