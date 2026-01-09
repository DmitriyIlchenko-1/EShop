using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Infrustructure;

public class StaticFilesStartup : IEStartup
{
    public int Order => PipelineOrder.StaticFilesMiddleware;
    public void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseStaticFiles();
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
         
    }
}