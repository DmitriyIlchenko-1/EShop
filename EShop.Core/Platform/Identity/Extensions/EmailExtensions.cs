using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Email;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Routing;

namespace EShop.Core.Platform.Identity.Extensions;

public static class EmailExtensions
{
    public static async Task SendEmailConfirmation(this IEmailService email, User user, string msg)
    {
        Guard.NotNull(email);
        Guard.NotNull(user);
        if (user.Email.IsEmpty())
        {
            return;
        }
        
        var from = EngineContext.Current.ApplicationContext.Configuration.Mail.Username;
        var client = await email.ConnectAsync();
        await client.SendAsync("Email Confirmation Letter", from, user.Email, msg);

    }
} 