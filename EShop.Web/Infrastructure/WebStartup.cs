using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Utilities;
using EShop.Web.Controllers;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EShop.Web.Infrastructure;

public class WebStartup : BaseStartup
{
    public override int Order => PipelineOrder.Default;
    

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>( );
        services.AddScoped<IUrlHelper>(x =>
        {
            var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
            var factory = x.GetRequiredService<IUrlHelperFactory>();
            return factory.GetUrlHelper(actionContext);
        });
        services.AddScoped<CatalogHelper>();
        services.AddScoped<CheckoutHelper>();
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