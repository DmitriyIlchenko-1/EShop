// using EasyCaching.Core;
// using EShop.Core.Platform.Caching;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Caching.Distributed;
// using ZiggyCreatures.Caching.Fusion;
//
// namespace EShop.Infrastructure.Caching.Adapters.Fusion;
//
// public class FusionHybridCacheManager : ICacheManager
// {
//     private readonly IFusionCache _fusionCache;
//
//     public FusionHybridCacheManager(IFusionCache cache)
//     {
//         _fusionCache = cache;
//     }
//
//
//     public bool HasDistributedProvider => _fusionCache.HasDistributedCache;
//
//     public async ValueTask<T> GetOrCreateAsync<T>(string key,
//         Func<CancellationToken, Task<T>> factory, CacheEntryOptions options = null,
//         IEnumerable<string> tags = null, CancellationToken cancellationToken = default)
//     {
//         _fusionCache.rem
//         FusionCacheEntryOptions fOptions = options.MapToFusionOptions();
//         return await _fusionCache.GetOrSetAsync(key: key,
//             factory: factory,
//             options: fOptions,
//             tags: tags,
//             cancellationToken);
//     }
//
//     public async ValueTask SetAsync<T>(string key, T value, CacheEntryOptions options = null,
//         IEnumerable<string> tags = null,
//         CancellationToken cancellationToken = default)
//     {
//         await _fusionCache.SetAsync(key, value, options.MapToFusionOptions(), tags, cancellationToken);
//     }
//
//     public async ValueTask RemoveAsync(string key, CacheEntryOptions options = null,
//         CancellationToken cancellationToken = default)
//     {
//         await _fusionCache.RemoveAsync(key, options.MapToFusionOptions(), cancellationToken);
//     }
//
//     public async ValueTask RemoveByTagAsync(string tag, CacheEntryOptions options = null,
//         CancellationToken cancellationToken = default)
//     {
//         await _fusionCache.RemoveByTagAsync(tag, options.MapToFusionOptions(), cancellationToken);
//     }
//
//     public async ValueTask ExpireAsync(string key, CacheEntryOptions options = null,
//         CancellationToken cancellationToken = default)
//     {
//         await _fusionCache.ExpireAsync(key, options.MapToFusionOptions(), cancellationToken);
//     }
// }