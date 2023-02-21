namespace Argon.QueryBuilder.Clauses;

public class OffsetClause : AbstractClause
{
    private long _offset;

    public long Offset
    {
        get => _offset;
        set => _offset = value > 0 ? value : _offset;
    }

    public bool HasOffset()
        => _offset > 0;

    public OffsetClause Clear()
    {
        _offset = 0;
        return this;
    }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new OffsetClause
        {
            Offset = Offset,
            Component = Component
        };
}
