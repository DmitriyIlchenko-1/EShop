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
    public IChildLifetimeScopeAccessor ChildLifetimeScopeAccessor { get; set; }
    public IHostEnvironment Environment { get; set; }

    public IEngineStartup Startup(IHostEnvironment environment)
    {
        Environment = environment;
        return new EShopEngineStartup(this);
    }

    public T? Resolve<T>() where T : class
        => ChildLifetimeScopeAccessor.GetChildLifetimeScope.Resolve<T>();

    public T ResolveOptional<T>() where T : class
        => ChildLifetimeScopeAccessor.GetChildLifetimeScope.ResolveOptional<T>();

    public object? Resolve(Type type)
        => ChildLifetimeScopeAccessor.GetChildLifetimeScope.Resolve(type);
}