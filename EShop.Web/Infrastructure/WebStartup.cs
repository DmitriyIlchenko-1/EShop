using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using EShop.Web.Controllers;

namespace EShop.Web.Infrastructure;

public class WebStartup : BaseStartup
{
    public override int Order => PipelineOrder.Default;
    

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CatalogHelper>();
    }
}