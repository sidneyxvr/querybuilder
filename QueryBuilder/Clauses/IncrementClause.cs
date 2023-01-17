namespace SqlKata;

public class IncrementClause : InsertClause
{
    public string Column { get; set; }
    public int Value { get; set; } = 1;

    public override AbstractClause Clone()
        => new IncrementClause
        {
            Engine = Engine,
            Component = Component,
            Column = Column,
            Value = Value
        };
}
