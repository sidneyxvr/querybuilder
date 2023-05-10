namespace Argon.QueryBuilder.Clauses;

public abstract class AbstractCondition : AbstractClause
{
    public bool IsOr { get; set; } = false;
    public bool IsNot { get; set; } = false;
}

/// <summary>
/// Represents a comparison between a column and a value.
/// </summary>
public class BasicCondition : AbstractCondition
{
    public required string Column { get; set; }
    public required string Operator { get; set; }
    public virtual required object Value { get; set; }
}

public class BasicStringCondition : BasicCondition
{
    private string? _escapeCharacter = null;
    public string? EscapeCharacter
    {
        get => _escapeCharacter;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                value = null;
            else if (value.Length > 1)
                throw new ArgumentOutOfRangeException($"The {nameof(EscapeCharacter)} can only contain a single character!");
            _escapeCharacter = value;
        }
    }
}

public class BasicDateCondition : BasicCondition
{
    public required string Part { get; set; }
}

/// <summary>
/// Represents a comparison between two columns.
/// </summary>
public class TwoColumnsCondition : AbstractCondition
{
    public required string First { get; set; }
    public required string Operator { get; set; }
    public required string Second { get; set; }
}

/// <summary>
/// Represents a comparison between a column and a full "subquery".
/// </summary>
public class QueryCondition<T> : AbstractCondition where T : BaseQuery<T>
{
    public required string Column { get; set; }
    public required string Operator { get; set; }
    public required Query Query { get; set; }
}

/// <summary>
/// Represents a comparison between a full "subquery" and a value.
/// </summary>
public class SubQueryCondition<T> : AbstractCondition where T : BaseQuery<T>
{
    public required object Value { get; set; }
    public required string Operator { get; set; }
    public required Query Query { get; set; }
}

/// <summary>
/// Represents a "is in" condition.
/// </summary>
public class InCondition<T> : AbstractCondition
{
    public required string Column { get; set; }
    public required IEnumerable<T> Values { get; set; }
}

/// <summary>
/// Represents a "is in subquery" condition.
/// </summary>
public class InQueryCondition : AbstractCondition
{
    public required Query Query { get; set; }
    public required string Column { get; set; }
}

/// <summary>
/// Represents a "is between" condition.
/// </summary>
public class BetweenCondition<T> : AbstractCondition
    where T : notnull
{
    public required string Column { get; set; }
    public required T Higher { get; set; }
    public required T Lower { get; set; }
}

/// <summary>
/// Represents an "is null" condition.
/// </summary>
public class NullCondition : AbstractCondition
{
    public required string Column { get; set; }
}

/// <summary>
/// Represents a boolean (true/false) condition.
/// </summary>
public class BooleanCondition : AbstractCondition
{
    public required string Column { get; set; }
    public required bool Value { get; set; }
}

/// <summary>
/// Represents a "nested" clause condition.
/// i.e OR (myColumn = "A")
/// </summary>
public class NestedCondition<T> : AbstractCondition where T : BaseQuery<T>
{
    public required T Query { get; set; }
}

/// <summary>
/// Represents an "exists sub query" clause condition.
/// </summary>
public class ExistsCondition : AbstractCondition
{
    public required Query Query { get; set; }
}
