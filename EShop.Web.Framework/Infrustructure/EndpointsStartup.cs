using EShop.Core.Platform.Routing;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Infrustructure;

public class EndpointsStartup : IEStartup
{
    public int Order => PipelineOrder.Late;

    public void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseEndpoints(p =>
        {
            p.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            p.MapControllerRoute("areas", "{area:exists}/{controller}/{action}/{id?}");
            p.MapDynamicControllerRoute<SlugRouteValueTransformer>("/{**slug:minlength(2)}");
        });
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}