using EasyCaching.Core.Configurations;
using EasyCaching.InMemory;
using EShop.Core.Platform.Caching;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Caching.Adapters.EasyCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ResourceManagement;

namespace EShop.Web.Common.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEasyCaching(configuration);

        services.AddSingleton<ICacheManagerFactory, EasyCachingManagerFactory>();

        //Lets us inject the hybrid cache without using the factory. 
        services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<ICacheManagerFactory>();
            return factory.GetHybridCache();
        });
    }

    private static void AddEasyCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEasyCaching(options =>
        {
            options.UseInMemory(
                config =>
                {
                    config.DBConfig = new InMemoryCachingOptions
                    {
                        EnableReadDeepClone = false,
                        EnableWriteDeepClone = false
                    };
                },
                CachingConstValue.MemoryCacheName);


            var storageConfig = configuration
                .GetSection("Redis")
                .Get<DistributedCacheSettings>();

            options.WithJson(CachingConstValue.DistributedCache);
            options.UseInMemory(
                config =>
                {
                    config.DBConfig = new InMemoryCachingOptions
                    {
                        EnableReadDeepClone = false,
                        EnableWriteDeepClone = false
                    };
                },
                "DistributedM1");
            options.UseRedis(conf =>
                {
                    conf.DBConfig.ConnectionTimeout = 10_000;
                    conf.DBConfig.Username = storageConfig.Username;
                    conf.DBConfig.Password = storageConfig.Password;
                    conf.DBConfig.Endpoints.Add(new ServerEndPoint
                    {
                        Host = storageConfig.Host,
                        Port = storageConfig.Port
                    });
                },
                CachingConstValue.DistributedCache);
            options.UseHybrid(x =>
                {
                    x.TopicName = "hybrid-cache";
                    x.LocalCacheProviderName = "DistributedM1";
                    x.DistributedCacheProviderName = CachingConstValue.DistributedCache;
                },
                CachingConstValue.HybridCacheName);
        });
    }

    // public static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    // {
    //     services.AddStackExchangeRedisCache(setup =>
    //     {
    //         //TODO: access through the config. 
    //         var config = configuration
    //             .GetSection("Redis")
    //             .Get<DistributedCacheSettings>();
    //
    //         var options = new ConfigurationOptions()
    //         {
    //             EndPoints = { config.Host },
    //             User = config.Username,
    //             Password = config.Password
    //         };
    //
    //         setup.InstanceName = config.InstanceName;
    //         setup.ConfigurationOptions = options;
    //     });
    // }

    // public static void AddFusionCacheServices(this IServiceCollection services, IConfiguration configuration)
    // {
    //     services.AddFusionCacheStackExchangeRedisBackplane(options =>
    //     {
    //         //TODO: access through the config. 
    //         var config = configuration
    //             .GetSection("Redis")
    //             .Get<DistributedCacheSettings>();
    //
    //         var settings = new ConfigurationOptions()
    //         {
    //             EndPoints = { config.Host },
    //             User = config.Username,
    //             Password = config.Password
    //         };
    //
    //         options.ConfigurationOptions = settings;
    //     });
    //
    //     services.AddSingleton<IFusionCacheSerializer, FusionCacheSerializer>();
    //
    //     services
    //         .AddFusionCache(FusionCacheFactory.HybridCacheName)
    //         .WithRegisteredSerializer()
    //         .WithRegisteredDistributedCache()
    //         .WithRegisteredBackplane();
    //
    //     services
    //         .AddFusionCache(FusionCacheFactory.MemoryCacheName)
    //         .WithRegisteredSerializer()
    //         .WithRegisteredMemoryCache()
    //         .WithoutDistributedCache();
    // }
    
    public static void AddResources(this IServiceCollection services)
    {
        services.AddResourceManagement();
        services.AddScoped<IResourcesTagHelperProcessor, ResourcesTagHelperProcessor>();
    }
}