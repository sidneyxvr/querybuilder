namespace Argon.QueryBuilder.Clauses;

public abstract class AbstractOrderBy : AbstractClause
{

}

public class OrderBy : AbstractOrderBy
{
    public required string Column { get; set; }
    public bool Ascending { get; set; } = true;
}

public class OrderByRandom : AbstractOrderBy
{
}
