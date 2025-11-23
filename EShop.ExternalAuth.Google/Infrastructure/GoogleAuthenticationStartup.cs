using EShop.Core.Platform.Identity.Bootstraping;
using EShop.ExternalAuth.Google.Configuration;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.ExternalAuth.Google.Infrastructure;

public class GoogleAuthenticationStartup : IExternalAuthenticationSetup
{
    public void Setup(AuthenticationBuilder builder)
    {
        builder.AddGoogle(GoogleDefaults.AuthenticationScheme,
            options =>
            {
                // var settings = EngineContext.Current.Resolve<GoogleExternalAuthSettings>();
                // options.ClientId = settings.ClientId;
                // options.ClientSecret = settings.ClientSecret;


                options.ClientId = "214332222627-grqb0di5mr3s8ermecp8lj9hd7mpc9re.apps.googleusercontent.com";
                options.ClientSecret = "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";
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");
                options.Scope.Add("https://www.googleapis.com/auth/user.gender.read");

                /*
                 *"ClientId": "214332222627-grqb0di5mr3s8ermecp8lj9hd7mpc9re.apps.googleusercontent.com",
                   "ClientSecret": "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"
                 *
                 */

                //options.SaveTokens = true; ??

                
                options.Events = new OAuthEvents
                {
                    OnRemoteFailure = context =>
                    {
                        context.HandleResponse();
                        string errorUrl =
                            $"/identity/externalerror?provider={GoogleDefaults.AuthenticationScheme}&error={Uri.EscapeDataString(context.Failure.Message)}";
                        context.Response.Redirect(errorUrl);
                        return Task.CompletedTask;
                    }
                };
            });
    }
}