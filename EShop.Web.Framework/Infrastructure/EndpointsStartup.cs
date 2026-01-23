using EShop.Core.Platform.Routing;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Infrastructure;

public class EndpointsStartup : BaseStartup
{
    public override int Order => PipelineOrder.Late;

    public override void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseEndpoints(p =>
        {
            p.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            p.MapControllerRoute("areas", "{area:exists}/{controller}/{action}/{id?}");
            p.MapDynamicControllerRoute<SlugRouteValueTransformer>("/{**slug:minlength(2)}");
        });
    }

    
}