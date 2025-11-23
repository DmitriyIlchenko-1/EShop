using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Catalog.Configuration;

public class CatalogSettings : ISettings
{
    public int RecentlyViewedProductsNumber { get; set; } = 8;
}