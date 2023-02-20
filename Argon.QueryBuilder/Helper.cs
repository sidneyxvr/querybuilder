using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace Argon.QueryBuilder;

public static partial class Helper
{


    //public static bool IsArray(object value)
    //    => value switch
    //    {
    //        string => false,
    //        byte[] => false,
    //        _ => value is IEnumerable
    //    };

    /// <summary>
    /// Flat IEnumerable one level down
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    //public static IEnumerable<object> Flatten(IEnumerable<object> array)
    //{
    //    foreach (var item in array)
    //    {
    //        if (IsArray(item))
    //        {
    //            foreach (var sub in (IEnumerable)item)
    //            {
    //                yield return sub;
    //            }
    //        }
    //        else
    //        {
    //            yield return item;
    //        }
    //    }
    //}

    //public static IEnumerable<object> FlattenDeep(IEnumerable<object> array)
    //    => array.SelectMany(o => IsArray(o) ? FlattenDeep((IEnumerable<object>)o) : new[] { o });

    public static IEnumerable<int> AllIndexesOf(string str, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield break;
        }

        var index = 0;

        do
        {
            index = str.IndexOf(value, index, StringComparison.Ordinal);

            if (index == -1)
            {
                yield break;
            }

            yield return index;

        } while ((index += value.Length) < str.Length);
    }

    public static string ReplaceAll(string subject, string match, Func<int, string> callback)
    {
        if (string.IsNullOrWhiteSpace(subject) || !subject.Contains(match))
        {
            return subject;
        }

        var splitted = subject.Split(match, StringSplitOptions.None);

        return splitted.Skip(1)
            .Select((item, index) => callback(index) + item)
            .Aggregate(new StringBuilder(splitted.First()), (prev, right) => prev.Append(right))
            .ToString();
    }

    //public static string ExpandParameters(string sql, string placeholder, object[] bindings)
    //    => ReplaceAll(sql, placeholder, i =>
    //    {
    //        var parameter = bindings[i];

    //        if (parameter is not string and IEnumerable array)
    //        {
    //            var first = true;
    //            foreach (var param in array)
    //            {
    //                if (!first)
    //                {

    //                }



    //                first = false;
    //            }
    //            return string.Join(',', placeholder.Repeat(count));
    //        }

    //        return placeholder.ToString();
    //    });

    //public static int EnumerableCount(IEnumerable obj)
    //{
    //    int count = 0;

    //    foreach (var item in obj)
    //    {
    //        count++;
    //    }

    //    return count;
    //}



    //public static IEnumerable<string> Repeat(this string str, int count)
    //    => Enumerable.Repeat(str, count);

    //public static string ReplaceIdentifierUnlessEscaped(this string input, string escapeCharacter, string identifier, string newIdentifier)
    //{
    //    //Replace standard, non-escaped identifiers first
    //    var nonEscapedRegex = new Regex($@"(?<!{Regex.Escape(escapeCharacter)}){Regex.Escape(identifier)}");
    //    var nonEscapedReplace = nonEscapedRegex.Replace(input, newIdentifier);

    //    //Then replace escaped identifiers, by just removing the escape character
    //    var escapedRegex = new Regex($@"{Regex.Escape(escapeCharacter)}{Regex.Escape(identifier)}");
    //    return escapedRegex.Replace(nonEscapedReplace, identifier);
    //}
}
