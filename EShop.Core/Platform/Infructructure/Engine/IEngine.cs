using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Infrastructure.Engine;

public interface IEngine
{
    IScopedProviderAccessor ScopeAccessor { get; set; }
    void ConfigureRequestPipeline(IApplicationBuilder appBuilder);
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    T? Resolve<T>(IServiceScope? scope = null) where T : class;
    object? Resolve(Type type, IServiceScope? scope = null);
}