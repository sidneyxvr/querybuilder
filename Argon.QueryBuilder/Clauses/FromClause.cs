namespace Argon.QueryBuilder.Clauses;

public abstract class AbstractFrom : AbstractClause
{
    protected string? _alias;

    /// <summary>
    /// Try to extract the Alias for the current clause.
    /// </summary>
    /// <returns></returns>
    public virtual string? Alias { get => _alias; set => _alias = value; }
}

/// <summary>
/// Represents a "from" clause.
/// </summary>
public class FromClause : AbstractFrom
{
    public required string Table { get; set; }

    public override string? Alias
        => Table.Contains(" as ", StringComparison.OrdinalIgnoreCase)
        ? Table.Split(' ', StringSplitOptions.RemoveEmptyEntries)[2]
        : Table;

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new FromClause
        {
            Engine = Engine,
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

    public override string? Alias
        => string.IsNullOrEmpty(_alias) ? Query.QueryAlias : _alias;

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new QueryFromClause
        {
            Engine = Engine,
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
            Engine = Engine,
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
            Engine = Engine,
            Alias = Alias,
            Columns = Columns,
            Values = Values,
            Component = Component
        };
}
