using EShop.Infrastructure.Domain;

namespace EShop.Core.Checkout.Orders.Domain;

public class OrderItem : BaseEntity
{
    public Guid OrderItemGuid { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal SubtotalWithDiscount { get; set; }
    public decimal SubtotalWithNoDiscount { get; set; }
    public decimal UnitPrice { get; set; }
    public string RawAttributes { get; set; }
     
}