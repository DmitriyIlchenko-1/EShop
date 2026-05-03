using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Infrastructure.Engine;

public interface IEngine
{
    bool IsStarted { get; }
    public IApplicationContext ApplicationContext { get; }
    IChildLifetimeScopeAccessor ChildLifetimeScopeAccessor { get; set; }
    public IHostEnvironment Environment { get; }
    IEngineStartup Startup(IApplicationContext applicationContext);
    T Resolve<T>() where T : class;
    T? ResolveOptional<T>() where T : class;
    object? Resolve(Type type);
}