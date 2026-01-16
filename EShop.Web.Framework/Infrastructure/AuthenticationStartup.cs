using EShop.Core.Data;
using EShop.Core.Platform.Identity.Bootstraping;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Services;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Http;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Infrustructure;

public class AuthenticationStartup : IEStartup
{
    public int Order => PipelineOrder.AuthMiddleware;

    public void ConfigureApplication(IApplicationBuilder app)
    {
       app.UseAuthentication();
       app.UseAuthorization();
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentity<User, Role>(options =>
            {
                
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;

                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddSignInManager<CustomSignInManager>()
            .AddDefaultTokenProviders()
            .AddUserStore<CustomUserStore>()
            .AddRoleStore<CustomRoleStore>();


        services.AddScoped(typeof(IRoleStore<>), typeof(RoleStore<>));

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = CookieNames.Identity;
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
            options.ReturnUrlParameter = "returnUrl";
        });

        var authenticationBuilder = new AuthenticationBuilder(services);
        var typeScanner = Singleton<ITypeScanner>.Instance;
        var externalAuthConfigurations = typeScanner.FindClassesOfType<IExternalAuthenticationSetup>();
        var externalAuthConfigurationInstances =
            externalAuthConfigurations.Select(x => (IExternalAuthenticationSetup)Activator.CreateInstance(x));
        foreach (var instance in externalAuthConfigurationInstances)
        {
            instance.Setup(authenticationBuilder);
        }
    }
}