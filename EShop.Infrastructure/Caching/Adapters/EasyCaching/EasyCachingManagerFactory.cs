using EasyCaching.Core;
using EShop.Core.Platform.Caching;

namespace EShop.Infrastructure.Caching.Adapters.EasyCaching;

public class EasyCachingManagerFactory : ICacheManagerFactory
{
    private readonly ICacheManager _memoryCache; //L1
    private readonly ICacheManager _hybridCache; // L1 + L2
    
    public EasyCachingManagerFactory(IEasyCachingProviderFactory easyCachingFactory, IHybridProviderFactory hybridFactory)
    {
        _hybridCache = new HybridEasyCachingManager(hybridFactory.GetHybridCachingProvider(CachingConstValue.HybridCacheName));
        _memoryCache = new HybridEasyCachingManager(easyCachingFactory.GetCachingProvider(CachingConstValue.MemoryCacheName));
    }

    public ICacheManager GetMemoryCache()
        => _memoryCache;
    
    public ICacheManager GetHybridCache()
        => _hybridCache;
}