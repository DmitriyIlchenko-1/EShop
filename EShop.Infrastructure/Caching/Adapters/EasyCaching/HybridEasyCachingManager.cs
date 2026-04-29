using EasyCaching.Core;
using EShop.Core.Platform.Caching;

namespace EShop.Infrastructure.Caching.Adapters.EasyCaching;

public class HybridEasyCachingManager : ICacheManager
{
    private readonly IEasyCachingProviderBase _cache;
    private readonly bool _hasDistributedProvider;
    public bool HasDistributedProvider => _hasDistributedProvider;

    public HybridEasyCachingManager(IEasyCachingProviderBase cache)
    {
        if (cache is IHybridCachingProvider || (cache is IEasyCachingProvider pr && pr.IsDistributedCache))
        {
            _hasDistributedProvider = true;
        }

        _cache = cache;
    }

     
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory,
        CacheEntryOptions options = null, CancellationToken cancellationToken = default)
        => (await _cache.GetAsync<T>(key,
            factory,
            options?.AbsoluteExpiration ?? TimeSpan.FromDays(999), //<--- TODO: This is a severe violation of the LSP. This is a temporary workaround. It does needs fixing!
            cancellationToken)).Value;

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