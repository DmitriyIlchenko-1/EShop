using EShop.Core.Catalog.Products.Price;
using EShop.Core.Checkout.Orders.Domain;
using EShop.Web.Common.Models;

namespace EShop.Web.Models.Account;

public class OrderListModel : BaseModel
{
    public ICollection<OrderModel> Orders { get; set; } = [];
}

public class OrderModel : BaseModel
{
    public string OrderStatus { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Subtotal { get; set; }
    
}

 