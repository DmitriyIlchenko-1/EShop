namespace EShop.Infrastructure.Email;

public interface IEmailService
{
    Task<ISmtpClient> ConnectAsync();
}

