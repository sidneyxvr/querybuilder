namespace Argon.QueryBuilder.Clauses;

public class LimitClause : AbstractClause
{
    private int _limit;

    public int Limit
    {
        get => _limit;
        set => _limit = value > 0 ? value : _limit;
    }

    public bool HasLimit()
        => _limit > 0;

    public LimitClause Clear()
    {
        _limit = 0;
        return this;
    }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new LimitClause
        {
            Limit = Limit,
            Component = Component
        };
}
