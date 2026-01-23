using System.Text.Json.Serialization;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Http;
using EShop.Infrastructure.Modules;
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

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
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