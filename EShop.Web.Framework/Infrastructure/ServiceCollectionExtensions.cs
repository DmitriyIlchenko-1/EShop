using System.Reflection;
using EShop.Core.Data.DbHandlers;
using EShop.Core.Data.DbHandlers.Configuration;
using EShop.Core.Platform.Caching;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Caching.Adapters.Fusion;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization;
using TypeExtensions = System.Reflection.TypeExtensions;

namespace EShop.Web.Common.Infrastructure;

public static class ServiceCollectionExtensions
{
    

    public static void AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

       // services.AddRedisCache(configuration);

        services.AddFusionCacheServices(configuration);

        services.AddSingleton<ICacheFactory, FusionCacheFactory>();

        //We can inject the hybrid cache without using the factory. 
        services.AddSingleton<ICacheManager>(sp =>
        {
            var factory = sp.GetRequiredService<ICacheFactory>();
            return factory.GetMemoryCache();
        });
    }

    public static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(setup =>
        {
            //TODO: access through the config. 
            var config = configuration
                .GetSection("Redis")
                .Get<DistributedCacheSettings>();

            var options = new ConfigurationOptions()
            {
                EndPoints = { config.Endpoint },
                User = config.User,
                Password = config.Password
            };

            setup.InstanceName = config.InstanceName;
            setup.ConfigurationOptions = options;
        });
    }

    public static void AddFusionCacheServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFusionCacheStackExchangeRedisBackplane(options =>
        {
            //TODO: access through the config. 
            var config = configuration
                .GetSection("Redis")
                .Get<DistributedCacheSettings>();

            var settings = new ConfigurationOptions()
            {
                EndPoints = { config.Endpoint },
                User = config.User,
                Password = config.Password
            };

            options.ConfigurationOptions = settings;
        });

        services.AddSingleton<IFusionCacheSerializer, FusionCacheSerializer>();

        services
            .AddFusionCache(FusionCacheFactory.HybridCacheName)
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        services
            .AddFusionCache(FusionCacheFactory.MemoryCacheName)
            .WithRegisteredSerializer()
            .WithRegisteredMemoryCache()
            .WithoutDistributedCache();
    }

    
}