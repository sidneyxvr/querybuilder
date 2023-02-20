using System.Text;

namespace Argon.QueryBuilder;

public class SqlResult
{
    public StringBuilder SqlBuilder { get; set; } = new StringBuilder();
    public Query Query { get; set; } = null!;
    public string RawSql { get; set; } = "";
    public string Sql { get; set; } = "";

    public Dictionary<string, object> NamedBindings = new();

    private int index = 0;

    public string GetParamName()
        => $"@p{index++}";
}
