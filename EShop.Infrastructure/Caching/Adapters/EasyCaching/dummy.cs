using EShop.Core.Platform.Caching;

namespace EShop.Infrastructure.Caching.Adapters.EasyCaching;

public class dummy  : ICacheManager
{
    public bool HasDistributedProvider { get; }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, CacheEntryOptions options = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(default(T));
    }

    public async Task SetAsync<T>(string key, T value, CacheEntryOptions options = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(default(T));
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(default(bool));
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotImplementedException();
    }
}