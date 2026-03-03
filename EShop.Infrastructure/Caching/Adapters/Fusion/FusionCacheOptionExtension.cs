// using ZiggyCreatures.Caching.Fusion;
//
// namespace EShop.Infrastructure.Caching.Adapters.Fusion;
//
// internal static class FusionCacheOptionExtension
// {
//     public static FusionCacheEntryOptions MapToFusionOptions(this CacheEntryOptions options)
//     {
//         var fusionOptions = new FusionCacheEntryOptions();
//
//         if (options.HardTtl.HasValue && options.SoftTtl.HasValue)
//         {
//             // With FusionCache we've got to configure a sliding expiration and an absolute expiration in a different way compared to Microsoft's APIs.
//             // https://github.com/ZiggyCreatures/FusionCache/discussions/63#discussioncomment-3047508
//             fusionOptions
//                 .SetDuration(options.SoftTtl.Value)
//                 .SetFailSafe(true, options.HardTtl.Value, options.AwaitFactoryCalltime)
//                 .SetEagerRefresh(options.EarlyRefreshThreshold)
//                 .SetFactoryTimeouts(options.FactoryTimeout);
//             
//        
//         }
//         else if(options.HardTtl.HasValue)
//         {
//             fusionOptions.SetDuration(options.HardTtl.Value);
//         }
//
//
//         return fusionOptions;
//     }
// }