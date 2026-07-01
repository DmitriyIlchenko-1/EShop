using EShop.Core.Platform.Configuration.Domain;

namespace EShop.ExternalAuth.Google.Configuration;

public class GoogleExternalAuthSettings : ISettings
{
    public string ClientId { get; set; } = "214332222627-grqb0di5mr3s8ermecp8lj9hd7mpc9re.apps.googleusercontent.com";
    public string ClientSecret { get; set; } = "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";
}