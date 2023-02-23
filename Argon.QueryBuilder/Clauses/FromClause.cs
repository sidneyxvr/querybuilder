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

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new FromClause
        {
            Alias = Alias,
            Table = Table,
            Component = Component,
        };
}

/// <summary>
/// Represents a "from subquery" clause.
/// </summary>
public class QueryFromClause : AbstractFrom
{
    public required Query Query { get; set; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new QueryFromClause
        {
            Alias = Alias,
            Query = Query.Clone(),
            Component = Component
        };
}

public class RawFromClause : AbstractFrom
{
    public required string Expression { get; set; }
    public required object[] Bindings { set; get; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new RawFromClause
        {
            Alias = Alias,
            Expression = Expression,
            Bindings = Bindings,
            Component = Component,
        };
}

/// <summary>
/// Represents a FROM clause that is an ad-hoc table built with predefined values.
/// </summary>
public class AdHocTableFromClause : AbstractFrom
{
    public required List<string> Columns { get; set; }
    public required List<object> Values { get; set; }

    public override AbstractClause Clone()
        => new AdHocTableFromClause
        {
            Alias = Alias,
            Columns = Columns,
            Values = Values,
            Component = Component
        };
}
