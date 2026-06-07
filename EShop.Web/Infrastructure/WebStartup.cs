using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Utilities;
using EShop.Web.Controllers;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace EShop.Web.Infrastructure;

public class WebStartup : BaseStartup
{
    public override int Order => PipelineOrder.Default;
    

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CatalogHelper>();
    }

    public override void ConfigureMvc(IMvcBuilder mvcBuilder, IServiceCollection services)
    {
        AddFluentValidation(services);
    }


    private static void AddFluentValidation(IServiceCollection services)
    {
        var typeScanner = Singleton<ITypeScanner>.Instance;
        services
            .AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssemblies(typeScanner.Assemblies);
        
    }
}