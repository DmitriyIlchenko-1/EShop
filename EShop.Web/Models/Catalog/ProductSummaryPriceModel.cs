using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Price;

namespace EShop.Web.Models.Catalog;

public class ProductSummaryPriceModel
{
    public Money FinalPrice { get; set; }
    public Money RegularPrice { get; set; }
    public PriceSaving Saving { get; set; }
}