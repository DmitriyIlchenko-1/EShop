using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using EShop.Infrastructure.Utilities;

namespace EShop.Infrastructure.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
        Func<TKey, TValue> valueFactory)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        if (dictionary is ConcurrentDictionary<TKey, TValue> c)
        {
            return c.GetOrAdd(key, valueFactory);
        }

        if (!dictionary.TryGetValue(key, out TValue value))
        {
            dictionary[key] = value = valueFactory(key);
        }

        return value;
    }


    public static bool TryGetValueAs<TValue>(this IDictionary<string, object?> dictionary, string key,
        [MaybeNullWhen(false)] out TValue value)
    {
        Guard.NotNull(dictionary);

        if (dictionary.TryGetValue(key, out object result) && result is TValue typedVal)
        {
            value = typedVal;
            return true;
        }

        value = default(TValue);
        return false;
    }

    public static TValue GetValueOrDefaultAs<TValue>(this IDictionary<string, object> dictionary,
        string key)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        dictionary.TryGetValueAs(key, out TValue result);
        return result;
    }
}