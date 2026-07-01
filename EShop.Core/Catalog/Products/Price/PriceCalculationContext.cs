using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data.Cart.Domain;

namespace EShop.Core.Catalog.Products.Price;

public class PriceCalculationContext
{
    public int Quantity { get; set; }
    public Product Product { get; set; }
    public ShoppingCartItem CartItem { get; set; }
    public ProductBatchContext BatchContext { get; set; }
}