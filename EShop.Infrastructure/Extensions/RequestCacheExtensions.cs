using System.Text.RegularExpressions;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;

namespace EShop.Caching;

public static class RequestCacheExtensions
{
    
    public static void RemoveByPattern(this IRequestCache cache, string pattern)
    {
        Guard.NotNull(cache);
        Guard.NotNull(pattern);
        if (pattern.IsEmpty()) return;
        var items = cache.Items;
        var keysToRemove = cache.SelectKeys(pattern);
        foreach (var key in keysToRemove)
        {
            items.Remove(key);
        }

    }

    public static IEnumerable<object> SelectKeys(this IRequestCache cache, string pattern)
    {
        var regex =  pattern == "*"
            ? null : new Regex(pattern, RegexOptions.IgnoreCase);

        foreach (var item in cache.Items)
        {
            if (item.Key is string str)
            {
                var matches = regex == null || regex.IsMatch(str);
                if (matches) yield return str;
            }
        }
    }
    
 
}