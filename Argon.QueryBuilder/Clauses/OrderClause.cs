namespace Argon.QueryBuilder.Clauses;

public abstract class AbstractOrderBy : AbstractClause
{

}

public class OrderBy : AbstractOrderBy
{
    public required string Column { get; set; }
    public bool Ascending { get; set; } = true;

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new OrderBy
        {
            Component = Component,
            Column = Column,
            Ascending = Ascending
        };
}

public class RawOrderBy : AbstractOrderBy
{
    public required string Expression { get; set; }
    public required object[] Bindings { set; get; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new RawOrderBy
        {
            Component = Component,
            Expression = Expression,
            Bindings = Bindings
        };
}

public class OrderByRandom : AbstractOrderBy
{
    /// <inheritdoc />
    public override AbstractClause Clone()
        => new OrderByRandom
        {
        };
}
