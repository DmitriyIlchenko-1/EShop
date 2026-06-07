using EShop.Infrastructure.Utilities;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace EShop.Infrastructure.Email;

public class MailKitSmtpClient : Disposable, ISmtpClient
{
   private readonly SmtpClient _client;
   private readonly MailAccount _mailAccount;

   public MailKitSmtpClient(SmtpClient client, MailAccount account)
   {
      Guard.NotNull(client);
      Guard.NotNull(account);
      _client = client;
      _mailAccount = account;
   }

   public async Task ConnectAsync()
   {
      try
      {

         await _client.ConnectAsync(_mailAccount.Host, _mailAccount.Port, (SecureSocketOptions)_mailAccount.SecureMailOptions);
         await _client.AuthenticateAsync(_mailAccount.Username, _mailAccount.Password);
         
      }
      catch (Exception e)
      {
         _client.Dispose();
         throw new InvalidOperationException(e.Message, e);
      }
   }

   public async Task SendAsync(string subject, string from, string to, string message, CancellationToken cancellationToken = default)
   {
      Guard.NotEmpty(message);
      CheckDisposed();
      var msg = DefaultEmailService.BuildMimeMessage(subject, from, to, message);
      await _client.SendAsync(msg, cancellationToken);
   }

   protected override async ValueTask DisposeAsync(bool disposing)
   {
      if (disposing)
      {
         if (_client.IsConnected)
         {
            await _client.DisconnectAsync(true);
         }

         _client.Dispose();
      }
   }
   protected override void Dispose(bool disposing)
   {
      if (disposing)
      {
         if (_client.IsConnected)
         {
             _client.Disconnect(true);
         }

         _client.Dispose();
      }
   }
}


public interface ISmtpClient
{
   Task ConnectAsync();

   Task SendAsync(string subject, string from, string to, string message,
      CancellationToken cancellationToken = default);
}

public class MailAccount
{
   public string Host { get; set; } 
   public int Port { get; set; } 
   public string Password { get; set; } 
   public string Username { get; set; } 
   public SecureMailOptions SecureMailOptions { get; set; } = SecureMailOptions.StartTlsWhenAvailable;
   
}
public enum SecureMailOptions
{
   None,
   Auto,
   SslOnConnect,
   StartTls,
   StartTlsWhenAvailable,
}
 