using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Content.Media.Configuration;

public class MediaSettings : ISettings
{
    public string ContentDeliveryNetwork { get; set; }
}