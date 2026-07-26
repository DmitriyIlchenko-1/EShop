using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;

namespace EShop.Infrastructure.Caching;

public class DefaultRequestCache : Disposable, IRequestCache
{
    private const string RequestCacheKey = "EShopRequestCacheKey";
    private readonly IHttpContextAccessor _httpContextAccessor;
    public DefaultRequestCache(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        Items = EnsureCreated();
    }

    public IDictionary<object, object> Items { get; }
    
    public T GetOrCreate<T>(object key, Func<T> factory)
    {
        if (Items.TryGetValue(key, out var result) && result is T typedRes)
        {
            return typedRes;
        }
        if (factory == null)
            return default;
        
        result = factory();
        Items[key] = result;
        return (T)result;
    }
    public async Task<T> GetOrCreateAsync<T>(object key, Func<Task<T>> factory)
    {
        if (Items.TryGetValue(key, out var result) && result is T typedRes)
        {
            return typedRes;
        }

        if (factory == null)
            return default;
        
        result = await factory();
        Items[key] = result;
        return (T)result;
    }

    public void Put(object key, object value)
    {
        Items[key] = value;
    }

    public bool TryGet<T>(object key, out T result)
    {
        if (Items.TryGetValue(key, out var res))
        {
            result = (T)res;
            return true;
        }

        result = default(T);
        return false;
    }

    public T Get<T>(object key)
    {
        return (T)Items[key];
    }

    public bool Contains(object key)
        => Items.ContainsKey(key);

    public void Remove(object key)
    {
        Items.Remove(key);
    }
    
    private IDictionary<object, object> EnsureCreated()
    {
        var items = _httpContextAccessor?.HttpContext?.Items;
        if (items is null)
            return new Dictionary<object, object>();
        
        if (items.TryGetValue(RequestCacheKey, out var requestCache) 
            && requestCache is Dictionary<object,object> d)
        {
            return d;
        }
        
        items[RequestCacheKey] = requestCache = new Dictionary<object, object>();
        return (Dictionary<object, object>) requestCache;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Items.Clear();
        }
    }
}