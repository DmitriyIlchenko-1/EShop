using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Infrustructure;

public class StaticFilesStartup : BaseStartup
{
    public override int Order => PipelineOrder.StaticFilesMiddleware;

    public override void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseStaticFiles();
    }
}