using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Infrustructure;

public class RoutingStartup : IEStartup
{
    public int Order => PipelineOrder.RoutingMiddleware;
    public void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseRouting();
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
      
    }
}