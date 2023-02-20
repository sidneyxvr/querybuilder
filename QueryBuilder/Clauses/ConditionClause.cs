using SqlKata;

namespace QueryBuilder.Clauses;

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

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new BasicCondition
        {
            Engine = Engine,
            Column = Column,
            Operator = Operator,
            Value = Value,
            IsOr = IsOr,
            IsNot = IsNot,
            Component = Component
        };
}

public class BasicStringCondition : BasicCondition
{

    public bool CaseSensitive { get; set; } = false;

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
    /// <inheritdoc />
    public override AbstractClause Clone()
        => new BasicStringCondition
        {
            Engine = Engine,
            Column = Column,
            Operator = Operator,
            Value = Value,
            IsOr = IsOr,
            IsNot = IsNot,
            CaseSensitive = CaseSensitive,
            EscapeCharacter = EscapeCharacter,
            Component = Component,
        };
}

public class BasicDateCondition : BasicCondition
{
    public required string Part { get; set; }

    /// <inheritdoc />
    public override AbstractClause Clone()
    => new BasicDateCondition
    {
        Engine = Engine,
        Column = Column,
        Operator = Operator,
        Value = Value,
        IsOr = IsOr,
        IsNot = IsNot,
        Part = Part,
        Component = Component
    };
}

/// <summary>
/// Represents a comparison between two columns.
/// </summary>
public class TwoColumnsCondition : AbstractCondition
{
    public required string First { get; set; }
    public required string Operator { get; set; }
    public required string Second { get; set; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new TwoColumnsCondition
        {
            Engine = Engine,
            First = First,
            Operator = Operator,
            Second = Second,
            IsOr = IsOr,
            IsNot = IsNot,
            Component = Component,
        };
}

/// <summary>
/// Represents a comparison between a column and a full "subquery".
/// </summary>
public class QueryCondition<T> : AbstractCondition where T : BaseQuery<T>
{
    public required string Column { get; set; }
    public required string Operator { get; set; }
    public required Query Query { get; set; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new QueryCondition<T>
        {
            Engine = Engine,
            Column = Column,
            Operator = Operator,
            Query = Query.Clone(),
            IsOr = IsOr,
            IsNot = IsNot,
            Component = Component,
        };
}

/// <summary>
/// Represents a comparison between a full "subquery" and a value.
/// </summary>
public class SubQueryCondition<T> : AbstractCondition where T : BaseQuery<T>
{
    public required object Value { get; set; }
    public required string Operator { get; set; }
    public required Query Query { get; set; }

    /// <inheritdoc />
    public override AbstractClause Clone()
    => new SubQueryCondition<T>
    {
        Engine = Engine,
        Value = Value,
        Operator = Operator,
        Query = Query.Clone(),
        IsOr = IsOr,
        IsNot = IsNot,
        Component = Component
    };
}

/// <summary>
/// Represents a "is in" condition.
/// </summary>
public class InCondition<T> : AbstractCondition
{
    public required string Column { get; set; }
    public required IEnumerable<T> Values { get; set; }
    public override AbstractClause Clone()
        => new InCondition<T>
        {
            Engine = Engine,
            Column = Column,
            Values = new List<T>(Values),
            IsOr = IsOr,
            IsNot = IsNot,
            Component = Component,
        };
}

/// <summary>
/// Represents a "is in subquery" condition.
/// </summary>
public class InQueryCondition : AbstractCondition
{
    public required Query Query { get; set; }
    public required string Column { get; set; }
    public override AbstractClause Clone()
        => new InQueryCondition
        {
            Engine = Engine,
            Column = Column,
            Query = Query.Clone(),
            IsOr = IsOr,
            IsNot = IsNot,
            Component = Component,
        };
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
    public override AbstractClause Clone()
        => new BetweenCondition<T>
        {
            Engine = Engine,
            Column = Column,
            Higher = Higher,
            Lower = Lower,
            IsOr = IsOr,
            IsNot = IsNot,
            Component = Component
        };
}

/// <summary>
/// Represents an "is null" condition.
/// </summary>
public class NullCondition : AbstractCondition
{
    public required string Column { get; set; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new NullCondition
        {
            Engine = Engine,
            Column = Column,
            IsOr = IsOr,
            IsNot = IsNot,
            Component = Component,
        };
}

/// <summary>
/// Represents a boolean (true/false) condition.
/// </summary>
public class BooleanCondition : AbstractCondition
{
    public required string Column { get; set; }
    public required bool Value { get; set; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new BooleanCondition
        {
            Engine = Engine,
            Column = Column,
            IsOr = IsOr,
            IsNot = IsNot,
            Component = Component,
            Value = Value
        };
}

/// <summary>
/// Represents a "nested" clause condition.
/// i.e OR (myColumn = "A")
/// </summary>
public class NestedCondition<T> : AbstractCondition where T : BaseQuery<T>
{
    public required T Query { get; set; }
    public override AbstractClause Clone()
        => new NestedCondition<T>
        {
            Engine = Engine,
            Query = Query.Clone(),
            IsOr = IsOr,
            IsNot = IsNot,
            Component = Component,
        };
}

/// <summary>
/// Represents an "exists sub query" clause condition.
/// </summary>
public class ExistsCondition : AbstractCondition
{
    public required Query Query { get; set; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new ExistsCondition
        {
            Engine = Engine,
            Query = Query.Clone(),
            IsOr = IsOr,
            IsNot = IsNot,
            Component = Component
        };
}

public class RawCondition : AbstractCondition
{
    public required string Expression { get; set; }
    public required object[] Bindings { set; get; }

    /// <inheritdoc />
    public override AbstractClause Clone()
        => new RawCondition
        {
            Engine = Engine,
            Expression = Expression,
            Bindings = Bindings,
            IsOr = IsOr,
            IsNot = IsNot,
            Component = Component
        };
}
