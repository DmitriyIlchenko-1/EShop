using System.Text.Json.Serialization;
using EShop.Infrastructure;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Http;
using EShop.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EShop.Web.Common.Infrustructure;

public class CommonStartup : IEStartup
{
    public int Order => PipelineOrder.AfterAuthMiddleware;

    public void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseSession();
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var mvcBuilder = services
            .AddControllersWithViews();
        mvcBuilder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
        mvcBuilder.AddSessionStateTempDataProvider();
        services.AddDistributedMemoryCache();
        services.AddHttpContextAccessor();

        services.AddSession(configure =>
        {
            configure.Cookie.Name = CookieNames.SessionCookie;
            configure.Cookie.HttpOnly = true;
            configure.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            configure.Cookie.IsEssential = true;
        });
    }
}