using EShop.Core.Catalog.Products.Domain;

namespace EShop.Core.Catalog.Products.Price;

public class PriceCalculationContext
{
    public Product Product { get; set; }
    public ProductBatchContext BatchContext { get; set; }
}