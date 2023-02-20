using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Argon.QueryBuilder.Exceptions;

public class CustomNullReferenceException : NullReferenceException
{
    public static void ThrowIfNull([NotNull] object? value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value is null)
        {
            throw new NullReferenceException(paramName);
        }
    }
}
