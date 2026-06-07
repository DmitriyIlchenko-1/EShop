using System.Collections.ObjectModel;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Utilities;
using MailKit.Net.Smtp;
using MimeKit;


namespace EShop.Infrastructure.Email;

public class DefaultEmailService : IEmailService
{
    private readonly IApplicationContext _app;

    public DefaultEmailService(IApplicationContext applicationContext, IApplicationContext app)
    {
        _app = app;
    }


    public virtual async Task<ISmtpClient> ConnectAsync()
    {
        var smtpClient = new SmtpClient();
        var mConfig = _app.Configuration.Mail;
        var client = new MailKitSmtpClient(smtpClient,
            new MailAccount()
            {
                Host = mConfig.Host,
                Port = mConfig.Port,
                Username = mConfig.Username,
                Password = mConfig.Password,
                SecureMailOptions = mConfig.SecureMailOptions,
               
            });
        await client.ConnectAsync();
        return client;
    }


    protected internal static MimeMessage BuildMimeMessage
        (string subject, string from, string to, string message)
    {
        Guard.NotEmpty(message);
        Guard.NotEmpty(subject);
        Guard.NotEmpty(to);
        Guard.NotEmpty(from);

        var msg = new MimeMessage()
        {
            Subject = subject,
        };

        msg.From.Add(new MailboxAddress(from.Substring(0, from.IndexOf('@', StringComparison.Ordinal)), from));
        msg.To.Add(new MailboxAddress(to.Substring(0, to.IndexOf('@', StringComparison.Ordinal)), to));
        msg.Body = new TextPart("plain")
        {
            Text = message
        };
        return msg;
    }
}