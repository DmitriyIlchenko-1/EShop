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
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Asn1.X509;
using UserStore = EShop.Core.Platform.Identity.Services.UserStore;

namespace EShop.Web.Common.Infrastructure;

public class AuthenticationStartup : BaseStartup
{
    public override int Order => PipelineOrder.AuthMiddleware;

    public override void ConfigureApplication(IApplicationBuilder app)
    {
       app.UseAuthentication();
       app.UseAuthorization();
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentity<User, Role>(options =>
            { 
                
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;

                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.User.RequireUniqueEmail = true;
                
            })
            .AddSignInManager<CustomSignInManager>()
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore>()
            .AddRoleStore<CustomRoleStore>();

        services.Configure<SecurityStampValidatorOptions>(opt =>
        {
            opt.ValidationInterval = TimeSpan.Zero;
        });

        services.AddScoped(typeof(IRoleStore<>), typeof(RoleStore<>));
        services.AddScoped<ILookupNormalizer, DefaultLookupNormalizer>();
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