
using EasyCaching.Core;
using EShop.Core.Platform.Caching;
using ZiggyCreatures.Caching.Fusion;

namespace EShop.Infrastructure.Caching.Adapters.Fusion;

public class FusionCacheFactory : ICacheFactory
{
    public const string HybridCacheName = "HybridCache";
    public const string MemoryCacheName = "MemoryCache";

    private readonly ICacheManager _memoryCache; //L1
    private readonly ICacheManager _hybridCache; // L1 + L2

    public FusionCacheFactory(IFusionCacheProvider cacheProvider)
    {
       
        _memoryCache
            = new FusionHybridCacheManager(cacheProvider.GetCache(MemoryCacheName));
        _hybridCache
            = new FusionHybridCacheManager(cacheProvider.GetCache(HybridCacheName));
    }


    public ICacheManager GetMemoryCache()
        => _memoryCache;

    public ICacheManager GetHybridCache()
        => _hybridCache;
}