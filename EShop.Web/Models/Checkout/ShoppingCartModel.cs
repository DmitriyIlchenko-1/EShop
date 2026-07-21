using EShop.Core.Checkout.Order.Domain;

namespace EShop.Web.Models.Checkout;

public class ShoppingCartModel
{
    public ShoppingCartSubtotal CartSubtotal { get; set; }
    public ICollection<ShoppingCartItemModel> Items { get; set; } = [];

}