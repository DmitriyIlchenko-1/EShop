using EasyCaching.Core;
using EShop.Core.Platform.Caching;

namespace EShop.Infrastructure.Caching.Adapters.EasyCaching;

public class HybridEasyCachingManager : ICacheManager
{
    private readonly IEasyCachingProviderBase _cache;
    private readonly bool _hasDistributedProvider;
    public bool HasDistributedProvider => _hasDistributedProvider;
    private readonly static TimeSpan MaxCacheLimit = TimeSpan.FromDays(365);

    public HybridEasyCachingManager(IEasyCachingProviderBase cache)
    {
        if (cache is IHybridCachingProvider || (cache is IEasyCachingProvider pr && pr.IsDistributedCache))
        {
            _hasDistributedProvider = true;
        }

        _cache = cache;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory,
        CancellationToken cancellationToken = default)
    {
        var options = new CacheEntryOptions(MaxCacheLimit);
        return await this.GetOrCreateAsync(key, factory, options, cancellationToken);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory,
        CacheEntryOptions options, CancellationToken cancellationToken = default)
        => (await _cache.GetAsync<T>(key,
            factory,
            options.AbsoluteExpiration,
            cancellationToken)).Value;
    
    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var options = new CacheEntryOptions(MaxCacheLimit);
        await this.SetAsync(key, value, options, cancellationToken);
    }

    public async Task SetAsync<T>(string key, T value, CacheEntryOptions options,
        CancellationToken cancellationToken = default)
        => await _cache.SetAsync<T>(key, value, options.AbsoluteExpiration, cancellationToken);


    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var result = await _cache.GetAsync<T>(key, cancellationToken);
        return result.HasValue ? result.Value : default(T);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var result = await _cache.ExistsAsync(key, cancellationToken);
        return result;
    }

    public async Task RemoveByPatternAsync(string pattern,
        CancellationToken cancellationToken = default(CancellationToken))
        => await _cache.RemoveByPatternAsync(pattern, cancellationToken);
}