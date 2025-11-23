using System.ComponentModel;

namespace EShop.Infrastructure.Extensions;

public static partial class StringExtensions
{
    public static int ToInt32(this string? value, int defaultValue = 0)
    {
        return !value.IsEmpty()
            ? Convert.ToInt32(value)
            : defaultValue;
    }

    public static bool IsEmpty(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
    
    public static bool HasValue(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public static string EmptyIfNull(this string? value)
    {
        return value ?? string.Empty;
    }
    
    public static string Reduce(this string str, int maxLength, string postFix = null)
    {
        if (str.IsEmpty())
        {
            return str;
        }

        if (str.Length <= maxLength)
        {
            return str;
        }

        int pLen = postFix?.Length ?? 0;

        var result = str.Substring(0, maxLength - pLen);
        if (!postFix.IsEmpty())
        {
            result += postFix;
        }

        return result;


    }
}