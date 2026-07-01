using System.Globalization;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Email;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EShop.Core.Platform.Identity.Extensions;

public static class EmailExtensions
{
    public static async Task SendEmailConfirmation(this IEmailService email, User user, string token)
    {
        Guard.NotNull(email);
        Guard.NotNull(user);
        if (user.Email.IsEmpty())
        {
            return;
        }
        
        var context = EngineContext.Current.Resolve<IHttpContextAccessor>()?.HttpContext;
        if (context == null)
        {
            return;
        }
       
        var g = EngineContext.Current.Resolve<LinkGenerator>();
        var path  = g.GetUriByAction(context, "ConfirmEmail", "Identity", new {token, email = user.Email});
        var msg = string.Format(CultureInfo.InvariantCulture, UserConstantTemplates.EmailConfirmation, path);
        var from = EngineContext.Current.ApplicationContext.Configuration.Mail.Username;
        var client = await email.ConnectAsync();
        await client.SendAsync("Email Confirmation Letter", from, user.Email, msg);

    }
    public static async Task SendResetPassword(this IEmailService email, User user, string token)
    {
        Guard.NotNull(email);
        Guard.NotNull(user);
        if (user.Email.IsEmpty())
        {
            return;
        }
        var context = EngineContext.Current.Resolve<IHttpContextAccessor>()?.HttpContext;
        
        if (context == null)
        {
            return;
        }
        var from = EngineContext.Current.ApplicationContext.Configuration.Mail.Username;
        var client = await email.ConnectAsync();
        var link = EngineContext.Current.Resolve<LinkGenerator>();
        var msg = string.Format(CultureInfo.InvariantCulture, UserConstantTemplates.EmailReset, link.GetUriByAction(context, "PasswordResetConfirm", "Identity", new {token, email = user.Email}));
        await client.SendAsync("Reset Password", from, user.Email, msg);

    }
} 