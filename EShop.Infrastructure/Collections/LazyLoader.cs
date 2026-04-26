namespace EShop.Infrastructure.Collections;



/// <summary>
/// This type is used as a value holder to support lazy loading.
/// It needs to be passed the necessary code to load its value when it's accessed. 
/// </summary>
public class LazyMultimap<T> : MultiMap<int, T>
{
    private readonly Func<int[], Task<MultiMap<int, T>>> _load;

    private readonly HashSet<int> _loadedKeys;

    private readonly HashSet<int> _unloadedKeys;

    public bool IsFullyLoaded { get; private set; }

    public LazyMultimap (
        Func<int[], Task<MultiMap<int, T>>> load, IEnumerable<int> keys = null)
    {
        _load = load;
        _loadedKeys = new HashSet<int>();
        _unloadedKeys = new HashSet<int>(keys);
    }

    public async Task<IEnumerable<T>> GetOrLoadAsync(int key)
    {
        if (key == 0)
        {
            return new HashSet<T>();
        }

        if (!_loadedKeys.Contains(key))
        {
            await LoadAsync(new int[]
            {
                key
            });
        }

        var result = base[key];
        return result;
    }

    public async Task LoadAllAsync()
    {
        IsFullyLoaded = true;
        await LoadAsync(_unloadedKeys);
        
    }

    private async Task LoadAsync(IEnumerable<int> keys)
    {
        if (keys == null)
        {
            return;
        }

        var keysToLoad = _unloadedKeys
            .Distinct()
            .Except(_loadedKeys)
            .ToArray();

        _unloadedKeys.Clear();

        if (keysToLoad.Any())
        {
            var items = await _load(keysToLoad);
            _loadedKeys.UnionWith(keysToLoad);

            if (items != null)
            {
                foreach (var item in items)
                {
                    base.AddRange(item.Key, item.Value);
                }
            }
        }
    }
}