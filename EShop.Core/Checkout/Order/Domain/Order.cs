using EShop.Core.Common.Domain;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Modules.Payment;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Checkout.Orders.Domain;

internal class OrderMap : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}


public class Order : BaseEntity, ISoftDeletableEntity, IAuditableEntity
{
    public Guid OrderGuid { get; set; }
    public int ShippingAddressId { get; set; }
    public Address ShippingAddress { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public string PaymentMethodSystemName { get; set; }
    public decimal Subtotal { get; set; }
    public decimal SubtotalRounded { get; set; }
    public decimal OrderDiscount { get; set; }
    public DateTime? PaidOnUtc { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public bool IsDeleted { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
}