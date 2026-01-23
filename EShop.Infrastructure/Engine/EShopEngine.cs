using Autofac;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Infrastructure.Engine;

public class EShopEngine : IEngine
{
    private IEStartup[] _startups;
    public IScopedProviderAccessor ScopeAccessor { get; set; }

    public EShopEngine()
    {
        LocateStartups();
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        // Modules ...

        foreach (var containerSetup in _startups.OfType<IContainerSetup>())
        {
            containerSetup.ConfigureContainer(builder);
        }
    }

    public void ConfigureRequestPipeline(IApplicationBuilder appBuilder)
    {
        foreach (var instance in _startups.OrderBy(s => s.Order))
        {
            instance.ConfigureApplication(appBuilder);
        }
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEngine>(this);
        
        foreach (var instance in _startups)
        {
            instance.ConfigureServices(services, configuration);
        }
    }

    public T? Resolve<T>(IServiceScope? scope = null) where T : class
    {
        return (T?)Resolve(typeof(T), scope);
    }

    public object? Resolve(Type type, IServiceScope? scope = null)
    {
        return GetServiceProvider(scope)
            ?
            .GetService(type);
    }

    protected IServiceProvider GetServiceProvider(IServiceScope? scope = null)
    {
        if (scope != null)
            return scope.ServiceProvider;

        // var accessor = _rootServiceProvider.GetService<IHttpContextAccessor>();
        // var context = accessor?.HttpContext;
        // return context?.RequestServices ?? _rootServiceProvider;
        return ScopeAccessor.GetScopedProvider;
    }

    private void LocateStartups()
    {
        var typeScanner = Singleton<ITypeScanner>.Instance;
        var startups = typeScanner.FindClassesOfType<IEStartup>();

        var instances = startups
            .Select(s => (IEStartup)Activator.CreateInstance(s))
            .Where(s => s != null);

        _startups = instances.ToArray();
    }
}