using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Data.Cart.Domain;

namespace EShop.Core.Checkout.Order.Domain;

public class ShoppingCartSubtotal
{
    public Money SubtotalWithNoDiscount { get; set; }
    public Money SubtotalWithDiscount { get; set; }
    public Money DiscountAmount { get; set; }
    public Discount DiscountApplied { get; set; }
    public ICollection<ShoppingCartItemLine> ShoppingCartLines { get; set; } = [];
}

public class ShoppingCartItemLine
{
    public ShoppingCartItemLine(ShoppingCartItem shoppingCartItem)
    {
        ShoppingCartItem = shoppingCartItem;
    }

    public ShoppingCartItem ShoppingCartItem { get; set; }
    public CalculatedPrice Subtotal { get; set; }
    public CalculatedPrice UnitPrice { get; set; }
}