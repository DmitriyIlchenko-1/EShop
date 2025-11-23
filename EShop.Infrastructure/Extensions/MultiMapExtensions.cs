using EShop.Infrastructure.Collections;

namespace EShop.Infrastructure.Extensions;

public static class MultiMapExtensions
{
    public static MultiMap<TKey, TValue> ToMultiMap<TSource, TKey, TValue>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        ArgumentNullException.ThrowIfNull(valueSelector, nameof(valueSelector));

        var map = new MultiMap<TKey, TValue>();
        foreach (var item in source)
        {
            map.Add(keySelector(item), valueSelector(item));
        }

        return map;
    }
}