using EShop.Core.Platform.Configuration.Domain;

namespace EShop.ExternalAuth.Google.Configuration;

public class GoogleExternalAuthSettings : ISettings
{
    public string ClientId { get; set; } = " ";
    public string ClientSecret { get; set; }  = " ";
}