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