using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Catalog.Configuration;

public class CatalogSettings : ISettings
{
    public int RecentlyViewedProductsNumber { get; set; } = 8;
    public int BrandCountOnHomePage { get; set; } = 5;
    public bool ShowVariantsProductList { get; set; }
    
    public int VariantCountProductList { get; set; } = 4;
    public bool ShowSku { get; set; }
    public bool ShowWeight { get; set; }
    public bool ShowDescriptionProductList { get; set; }
    public bool ShowBrandProductList { get; set; }
    public bool ShowSpecificationsProductList { get; set; }
    public bool ShowReviewsProductList { get; set; }
    
}