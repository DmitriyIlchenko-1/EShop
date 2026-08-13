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

    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken = default);

    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory,
        CacheEntryOptions options, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value,
        CacheEntryOptions options, CancellationToken cancellationToken = default);

    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern,
        CancellationToken cancellationToken = default(CancellationToken));
}