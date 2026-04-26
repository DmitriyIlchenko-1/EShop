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
    }
    
    public T GetOrCreate<T>(object key, Func<T> factory)
    {
        var requestCache = EnsureCreated();
        if (requestCache.TryGetValue(key, out var result) && result is T typedRes)
        {
            return typedRes;
        }
        if (factory == null)
            return default;
        
        result = factory();
        requestCache[key] = result;
        return (T)result;
    }
    public async Task<T> GetOrCreateAsync<T>(object key, Func<Task<T>> factory)
    {
        var requestCache = EnsureCreated();
        if (requestCache.TryGetValue(key, out var result) && result is T typedRes)
        {
            return typedRes;
        }

        if (factory == null)
            return default;
        
        result = await factory();
        requestCache[key] = result;
        return (T)result;
    }

    public void Put(object key, object value)
    {
        var requestCache = EnsureCreated();
        requestCache[key] = value;
    }

    public bool TryGet<T>(object key, out T result)
    {
        var requestCache = EnsureCreated();
        if (requestCache.TryGetValue(key, out var res) && res is T typedRes)
        {
            result = typedRes;
            return true;
        }

        result = default(T);
        return false;
    }

    public T Get<T>(object key)
    {
        var requestCache = EnsureCreated();
        return (T)requestCache[key];
    }

    public bool Contains(object key)
        => EnsureCreated()
            .ContainsKey(key);
    
    

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
            EnsureCreated()
                .Clear();
        }
    }
}