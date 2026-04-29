using System.Text.Json.Serialization;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Http;
using EShop.Infrastructure.Modules;
using EShop.Web.Common.Razor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Infrastructure;

public class CommonStartup : BaseStartup
{
    public override int Order { get; } = PipelineOrder.AfterAuthMiddleware;

    public override void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseSession();
        
    }

    public override void ConfigureMvc(IMvcBuilder builder, IServiceCollection services)
    { 
        
        builder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
        
        builder.AddRazorOptions(x =>
        {
            x.ViewLocationExpanders.Add(new ThemeLocationExpander());
        });
        
        builder.AddSessionStateTempDataProvider();
        
        services.AddSession(configure =>
        {
            configure.Cookie.Name = CookieNames.SessionCookie;
            configure.Cookie.HttpOnly = true;
            configure.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            configure.Cookie.IsEssential = true;
        });
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();
        services.AddSingleton<IChildLifetimeScopeAccessor, DefaultChildLifetimeScopeAccessor>();
    }
}