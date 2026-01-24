using Autofac;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Infrastructure.Engine;

public class EShopEngine : IEngine
{
    private IEStartup[] _startups;
    public IScopedProviderAccessor ScopeAccessor { get; set; }
    public IHostEnvironment Environment { get; set; }

    public IEngineStartup Startup(IHostEnvironment environment)
    {
        Environment = environment;
        return new EShopEngineStartup(this);
        
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

    
}