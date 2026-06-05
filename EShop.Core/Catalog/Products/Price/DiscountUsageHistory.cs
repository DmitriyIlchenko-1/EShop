using EShop.Core.Checkout.Order.Domain;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Catalog.Products.Price;

public class DiscountUsageHistory : BaseEntity
{
    public Discount Discount { get; set; }
    public int DiscountId { get; set; }
    public Order Order { get; set; }
    public int OrderId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}