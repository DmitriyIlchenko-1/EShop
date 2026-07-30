using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Checkout.Orders.Domain;

public class OrderItem : BaseEntity
{
    public Guid OrderItemGuid { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
    public decimal SubtotalRounded { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitPriceRounded { get; set; }
    public string RawAttributes { get; set; }
    public Product Product { get; set; }
     
}