namespace Argon.QueryBuilder;

public class SqlResult
{
    public required string Sql { get; init; }
    public required IReadOnlyDictionary<string, object> Parameters { get; init; }
}
