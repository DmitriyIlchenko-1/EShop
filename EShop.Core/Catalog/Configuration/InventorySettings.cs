using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Catalog.Configuration;

public class InventorySettings : ISettings
{
    public int InStockThreshold { get; set; } = 20;
}



 