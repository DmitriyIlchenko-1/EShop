using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Price;

namespace EShop.Web.Models.Checkout;

public class CartItemPriceModel
{
    public Money UnitPrice { get; set; }
    public Money Subtotal { get; set; }
    public Money Discount { get; set; }
    public PriceSaving UnitSaving { get; set; }
    public PriceSaving SubtotalSaving { get; set; }
    
}