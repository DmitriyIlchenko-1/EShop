using System.Web;
using EShop.Core.Platform.Identity.Bootstraping;
using EShop.ExternalAuth.Google.Configuration;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Extensions;
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
                var settings = EngineContext.Current.Resolve<GoogleExternalAuthSettings>();
                options.ClientId = settings.ClientId;
                options.ClientSecret = settings.ClientSecret;

                // These are set by default inside the built-in Google handler.
                // options.Scope.Add("openid");
                // options.Scope.Add("profile");
                // options.Scope.Add("email");

                options.Events = new OAuthEvents
                {
                    OnRemoteFailure = context =>
                    {
                        context.HandleResponse();
                        string errorUrl =
                            $"/identity/externalerror?provider={GoogleDefaults.AuthenticationScheme}";
                        if (context.Failure != null)
                        {
                            var errorType = context.Failure.Data.Contains("error")
                                ? context.Failure.Data["error"].ToString() : null;
                            if (!errorType.IsEmpty())
                            {
                                errorUrl += $"&errorType={Uri.EscapeDataString(errorType)}";
                            }

                            if (!context.Failure.Message.IsEmpty())
                            {
                                errorUrl += $"&error={Uri.EscapeDataString(context.Failure.Message)}";
                            }
                            
                        }
                        context.Response.Redirect(errorUrl);
                        return Task.CompletedTask;
                    }
                };
            });
    }
}