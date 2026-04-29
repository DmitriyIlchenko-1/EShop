using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Data;

public class StoreGeneralSettings : ISettings
{
    public string DefaultThemeName { get; set; }
}