using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Platform.Routing.Settings;

public class SeoSettings : ISettings
{
    public bool LoadAllUrlSlugsOnStartup { get; set; } = true;
}