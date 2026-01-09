using EShop.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace EShop.Core.Platform.Caching;

/// <summary>
/// It's a wrapper around whatever cache implementation we choose.
/// We don't use the HybridCache interface instead in case we decide to extend the public cache api,
/// which we wouldn't be able to do with HybridCache because it isn't ours.
/// </summary>
public interface ICacheManager
{
    bool HasDistributedProvider { get; }
    
    ValueTask<T> GetOrCreateAsync<T>(string key,
        Func<CancellationToken, Task<T>> factory, CacheEntryOptions options = null,
        IEnumerable<string> tags = null, CancellationToken cancellationToken = default);

    ValueTask SetAsync<T>(string key, T value, CacheEntryOptions options = null,
        IEnumerable<string> tags = null,
        CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(string key, CacheEntryOptions options = null,
        CancellationToken cancellationToken = default);

    ValueTask RemoveByTagAsync(string tag, CacheEntryOptions options = null,
        CancellationToken cancellationToken = default);

    ValueTask ExpireAsync(string key, CacheEntryOptions options = null,
        CancellationToken cancellationToken = default);
}

 