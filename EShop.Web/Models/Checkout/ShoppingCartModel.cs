 

using EShop.Core.Checkout.Orders.Domain;

namespace EShop.Web.Models.Checkout;

public class ShoppingCartModel
{
    public ShoppingCartSubtotal CartSubtotal { get; set; }
    public ICollection<ShoppingCartItemModel> Items { get; set; } = [];

}