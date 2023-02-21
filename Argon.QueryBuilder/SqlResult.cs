using System.Text;

namespace Argon.QueryBuilder;

public class SqlResult
{
    public StringBuilder SqlBuilder { get; set; } = new StringBuilder();
    public Query Query { get; set; } = null!;
    public string RawSql { get; set; } = "";

    public Dictionary<string, object> NamedBindings = new();

    private int index = 0;

    public string GetParamName()
        => $"@p{index++}";

    public void AddParam(object value)
        => NamedBindings.Add(GetParamName(), value);

    public void AddParams(object value)
        => NamedBindings.Add(GetParamName(), value);
}
