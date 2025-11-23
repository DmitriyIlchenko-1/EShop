using System.Reflection;
using EShop.Core.Platform.Configuration.Domain;
using EShop.Core.Platform.Configuration.Services;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Types;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Infrustructure;

public static class ServiceCollectionExtensions
{
    public static void ConfigureApplicationServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        GlobalConfiguration.ContentRootPath = builder.Environment.ContentRootPath;
        services.AddModules();
        
        var typeScanner = new DefaultTypeScanner();
        Singleton<ITypeScanner>.Instance = typeScanner;
        services.AddSingleton<ITypeScanner>(typeScanner);
        
        IEngine engine = EngineContext.Create();
        engine.ConfigureServices(services, builder.Configuration);
    }

    public static IServiceCollection AddModules(this IServiceCollection services)
    {
        foreach (ModuleInfo moduleInfo in ModuleManager.LoadModules())
        {
            moduleInfo.Assembly = Assembly.Load(new AssemblyName(moduleInfo.Name));
            GlobalConfiguration.Modules.Add(moduleInfo);
        }

        return services;
    }
}