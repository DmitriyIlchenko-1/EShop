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
        CacheEntryOptions options, CancellationToken cancellationToken = default)
        => (await _cache.GetAsync<T>(key, factory, options.AbsoluteExpiration, cancellationToken)).Value;

    public async Task SetAsync<T>(string key, T value, CacheEntryOptions options,
        CancellationToken cancellationToken = default)
        => await _cache.SetAsync<T>(key, value, options.AbsoluteExpiration, cancellationToken);

    public async Task RemoveByPatternAsync(string pattern,
        CancellationToken cancellationToken = default(CancellationToken))
        => await _cache.RemoveByPatternAsync(pattern, cancellationToken);
}