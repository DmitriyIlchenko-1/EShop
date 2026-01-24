using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Infrastructure.Engine;

public interface IEngine
{
    IScopedProviderAccessor ScopeAccessor { get; set; }
    public IHostEnvironment Environment { get; set; }
    IEngineStartup Startup(IHostEnvironment environment);
    T? Resolve<T>(IServiceScope? scope = null) where T : class;
    object? Resolve(Type type, IServiceScope? scope = null);
}