namespace SqlKata;

public class UnsafeLiteral
{
    public string Value { get; set; }

    public UnsafeLiteral(string value, bool replaceQuotes = true)
    {
        value ??= "";

        if (replaceQuotes)
        {
            value = value.Replace("'", "''");
        }

        Value = value;
    }

}
