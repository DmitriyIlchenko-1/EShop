using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using EShop.Web.Controllers;

namespace EShop.Web.Infrastructure;

public class WebStartup : IEStartup
{
    public int Order => PipelineOrder.Default;
    public void ConfigureApplication(IApplicationBuilder app)
    {
        
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CatalogHelper>();
    }
}