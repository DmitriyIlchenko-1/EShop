namespace EShop.Web.Models.Catalog;

public class ProductWidgetComponentModel
{
    public long Id { get; set; }

    public ICollection<ProductThumbnail> Products { get; set; }

    public ProductWidgetSetting Setting { get; set; }

    public string WidgetName { get; set; }
}