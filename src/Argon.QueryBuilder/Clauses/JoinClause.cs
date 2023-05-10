namespace Argon.QueryBuilder.Clauses;

public abstract class AbstractJoin : AbstractClause
{

}

public class BaseJoin : AbstractJoin
{
    public required Join Join { get; set; }
}
