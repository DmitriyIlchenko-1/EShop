using EasyCaching.Core;
using EShop.Core.Platform.Caching;

namespace EShop.Infrastructure.Caching.Adapters.EasyCaching;

public class EasyCachingFactory : ICacheFactory
{
    public const string HybridCacheName = "h1";
    public const string MemoryCacheName = "l1";
    public const string DistributedCache = "l2";
    private readonly ICacheManager _memoryCache; //L1
    private readonly ICacheManager _hybridCache; // L1 + L2
    

    public EasyCachingFactory(IEasyCachingProviderFactory easyCachingFactory, IHybridProviderFactory hybridFactory)
    {
        _hybridCache = new HybridEasyCachingManager(hybridFactory.GetHybridCachingProvider(HybridCacheName));
        _memoryCache = new HybridEasyCachingManager(easyCachingFactory.GetCachingProvider(MemoryCacheName));
    }

    public ICacheManager GetMemoryCache()
        => _memoryCache;
    
    public ICacheManager GetHybridCache()
        => _hybridCache;
}