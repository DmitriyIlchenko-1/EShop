using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace EShop.Infrastructure.Caching;

/// <summary>
/// 
/// </summary>
public class CacheEntryOptions
{
    public TimeSpan? HardTtl { get; set; }  
    public TimeSpan? SoftTtl { get; set; }


    /// <summary>
    /// Used to specify how often we want to call the factory in the background after the cache entry becomes expired and is now being used as a fallback till we can get a fresh value.
    /// Having it set to null means going and checking the database every subsequent request after a fail.
    /// </summary>
    public TimeSpan? AwaitFactoryCalltime { get; set; }

    /// <summary>
    /// Defines how long we wait before starting to use the expired value as a fallback if the downstream service isn't responding / slow and let the factory continue to execute in the background.
    /// </summary>
    public TimeSpan? FactoryTimeout { get; set; } 

    /// <summary>
    /// 0.1f = 10% of what's in the SoftTtl.
    /// The first request coming to the server when the set value is equal or greater than the specified percentage of <see cref="SoftTtl"/> will set off a background refresh to get a fresh value without slowing down the main flow.
    /// This is essentially loading of a new value in advance before the current entry expires.
    ///
    /// Note: It's only if a request comes to the server after the threshold that the background loading will begin. The 'pre-loading' won't start automatically until a request shows up.
    /// </summary>
    public float? EarlyRefreshThreshold { get; set; } 
}

public static partial class CacheEntryOptionsExtensions
{
    public static MemoryCacheEntryOptions MapToMemoryOptions(CacheEntryOptions options)
    {
        return new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.HardTtl,
            SlidingExpiration = options.SoftTtl,
        };
    }

    public static DistributedCacheEntryOptions MapToDistributedOptions(CacheEntryOptions options)
    {
        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.HardTtl,
            SlidingExpiration = options.SoftTtl
        };
    }
}