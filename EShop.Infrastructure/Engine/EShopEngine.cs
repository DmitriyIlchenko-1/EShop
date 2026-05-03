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
    public bool IsStarted { get; private set; }
    public IApplicationContext ApplicationContext { get; private set; }
    public IChildLifetimeScopeAccessor ChildLifetimeScopeAccessor { get; set; }
    public IHostEnvironment Environment { get; private set; }
    public IEngineStartup Startup(IApplicationContext applicationContext)
    {
        Guard.NotNull(applicationContext);
        Environment = applicationContext.Environment;
        ApplicationContext = applicationContext;
        IsStarted = true;
        return new EShopEngineStartup(this);
    }

    public T? Resolve<T>() where T : class
        => ChildLifetimeScopeAccessor.GetChildLifetimeScope.Resolve<T>();

    public T ResolveOptional<T>() where T : class
        => ChildLifetimeScopeAccessor.GetChildLifetimeScope.ResolveOptional<T>();

    public object? Resolve(Type type)
        => ChildLifetimeScopeAccessor.GetChildLifetimeScope.Resolve(type);
}