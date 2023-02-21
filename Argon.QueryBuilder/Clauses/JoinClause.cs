namespace Argon.QueryBuilder.Clauses;

public abstract class AbstractJoin : AbstractClause
{

}

public class BaseJoin : AbstractJoin
{
    public required Join Join { get; set; }

    public override AbstractClause Clone()
        => new BaseJoin
        {
            Join = Join.Clone(),
            Component = Component,
        };
}

public class DeepJoin : AbstractJoin
{
    public required string Type { get; set; }
    public required string Expression { get; set; }
    public required string SourceKeySuffix { get; set; }
    public required string TargetKey { get; set; }
    public required Func<string, string> SourceKeyGenerator { get; set; }
    public required Func<string, string> TargetKeyGenerator { get; set; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new DeepJoin
        {
            Component = Component,
            Type = Type,
            Expression = Expression,
            SourceKeySuffix = SourceKeySuffix,
            TargetKey = TargetKey,
            SourceKeyGenerator = SourceKeyGenerator,
            TargetKeyGenerator = TargetKeyGenerator,
        };
}
