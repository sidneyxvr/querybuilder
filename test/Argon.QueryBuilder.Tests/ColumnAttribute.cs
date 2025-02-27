namespace Argon.QueryBuilder.Tests;

/// <summary>
/// This class is used as metadata on a property to generate different name in the output query.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : Attribute
{
    public string Name { get; private set; }
    public ColumnAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
    }
}

/// <summary>
/// This class is used as metadata on a property to determine if it is a primary key
/// </summary>
public class KeyAttribute : ColumnAttribute
{
    public KeyAttribute([System.Runtime.CompilerServices.CallerMemberName] string name = "")
    : base(name)
    {

    }
}
