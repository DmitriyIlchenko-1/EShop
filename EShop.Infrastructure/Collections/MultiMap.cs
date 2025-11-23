namespace EShop.Infrastructure.Collections;

public class MultiMap<TKey, TValue> : Dictionary<TKey, HashSet<TValue>>
{
    public MultiMap() : base()
    {
    }

    public new HashSet<TValue>? this[TKey key] => GetValues(key);

    public void Add(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        HashSet<TValue> container = GetOrCreateContainer(key);
        container.Add(value);
    }

    public void AddRange(TKey key, IEnumerable<TValue> values)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        HashSet<TValue> container = GetOrCreateContainer(key);
        container.UnionWith(values);
    }

    public bool ContainsValue(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        HashSet<TValue> container = GetOrCreateContainer(key);
        return container.Contains(value);
    }

    public void Remove(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        HashSet<TValue> container = GetOrCreateContainer(key);

        container.Remove(value);
        if (container.Count == 0)
        {
            this.Remove(key);
        }
    }

    public HashSet<TValue>? GetValues(TKey key, bool canBeEmpty = true)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        HashSet<TValue> container;
        if (!TryGetValue(key, out container) && canBeEmpty)
        {
            container = new HashSet<TValue>();
        }

        return container;
    }
    
    private HashSet<TValue> GetOrCreateContainer(TKey key)
    {
        HashSet<TValue> container;
        if (!TryGetValue(key, out container))
        {
            container = new HashSet<TValue>();
            base.Add(key, container);
        }

        return container;
    }
}