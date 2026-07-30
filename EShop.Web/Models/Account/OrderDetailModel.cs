using EShop.Core.Catalog.Products.Price;
using EShop.Core.Checkout.Orders.Domain;
using EShop.Web.Common.Models;

namespace EShop.Web.Models.Account;

public class OrderDetailModel : BaseModel
{
    public AddressSummaryModel AddressModel { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderStatus { get; set; }
    public decimal Subtotal { get; set; }
    public string ShippingMethodName { get; set; }
    public string PaymentMethodName { get; set; }
    public ICollection<OrderItemModel> OrderItems { get; set; } = [];
}

