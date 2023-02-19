using System.Collections;
using System.Globalization;

namespace SqlKata;

public class SqlResult
{
    public Query Query { get; set; } = null!;
    public string RawSql { get; set; } = "";
    public List<object> Bindings { get; set; } = new();
    public string Sql { get; set; } = "";
    public Dictionary<string, object> NamedBindings = new();

    private static readonly Type[] NumberTypes =
    {
        typeof(int),
        typeof(long),
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(short),
        typeof(ushort),
        typeof(ulong),
    };

    public override string ToString()
    {
        var deepParameters = Helper.Flatten(Bindings).ToList();

        return Helper.ReplaceAll(RawSql, "?", i =>
        {
            if (i >= deepParameters.Count)
            {
                throw new Exception(
                    $"Failed to retrieve a binding at index {i}, the total bindings count is {Bindings.Count}");
            }

            var value = deepParameters[i];
            return ChangeToSqlValue(value);
        });
    }

    private static string ChangeToSqlValue(object value)
    {
        if (value is null)
        {
            return "NULL";
        }

        if (Helper.IsArray(value))
        {
            return string.Join(',', (IEnumerable)value);
        }

        if (NumberTypes.Contains(value.GetType()))
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture)!;
        }

        return value switch
        {
            DateTime date => date == date.Date
                ? "'" + date.ToString("yyyy-MM-dd") + "'"
                : "'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "'",
            bool b => b
                ? "true"
                : "false",
            Enum e => Convert.ToInt32(e) + $" /* {e} */",
            _ => "'" + value.ToString()!.Replace("'", "''") + "'"
        };
    }
}
