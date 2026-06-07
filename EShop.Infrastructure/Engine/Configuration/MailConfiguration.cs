using EShop.Infrastructure.Email;

namespace EShop.Infrastructure.Engine.Configuration;

public class MailConfiguration
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Password { get; set; }
    public string Username { get; set; }
    public SecureMailOptions SecureMailOptions { get; set; }
    
}