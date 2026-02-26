using System.Collections.Concurrent;

namespace EShop.Infrastructure.Extensions;

public static class DictionaryExtensions
{
    public static bool TryGetValueAs<TKey, TValue, TActual>(this IDictionary<TKey, TValue> dictionary, TKey key,
        out TActual value)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (dictionary.TryGetValue(key, out TValue result) && result is TActual typedVal)
        {
            value = typedVal;
            return true;
        }

        value = default(TActual);
        return false;
    }

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

    public static bool TryGetTypedValue<TKey, TValue, TActual>(this IDictionary<TKey, TValue> dictionary, TKey key,
        out TActual value) where TActual : TValue
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (dictionary.TryGetValue(key, out TValue result))
        {
            value = (TActual)result;
            return true;
        }

        value = default(TActual);
        return false;
    }

    public static TActual GetValueOrDefaultAs<TKey, TValue, TActual>(this IDictionary<TKey, TValue> dictionary,
        TKey key)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        return dictionary.TryGetValueAs(key, out TActual result) ? result : default(TActual);
    }
}