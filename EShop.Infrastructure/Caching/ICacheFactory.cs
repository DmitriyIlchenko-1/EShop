using EShop.Core.Platform.Caching;

namespace EShop.Infrastructure.Caching;

/// <summary>
/// This interface relies on its implementations to define the factory methods so that it returns an instance of the appropriate ICacheManager.
/// We need this interface to encapsulate the knowledge of which cache implementation to create and move this knowledge out of the framework.
/// If we ever decide to use another implementation, we simply implement this factory to change the concrete type behind ICacheManager.
///
/// We don't need to bind application-specific classes into our code.
/// The code only deals with the ICacheManager interface and so it can work with any user-defined 'ConcreteCacheManager' classes.
/// </summary>
public interface ICacheFactory
{
    ICacheManager GetMemoryCache(); 
    ICacheManager GetHybridCache();
}