using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Infrastructure;

public class RoutingStartup : BaseStartup
{
    public override int Order => PipelineOrder.RoutingMiddleware;
    public override void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseRouting();
    }

    
}