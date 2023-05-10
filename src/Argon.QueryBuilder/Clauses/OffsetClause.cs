namespace Argon.QueryBuilder.Clauses;

public class OffsetClause : AbstractClause
{
    private long _offset;

    public long Offset
    {
        get => _offset;
        set => _offset = value > 0 ? value : _offset;
    }
}
