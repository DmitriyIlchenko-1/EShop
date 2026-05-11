using System.ComponentModel.DataAnnotations;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Checkout.Shipping.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Products.Price;

public enum DiscountType
{
    ProductDiscount = 0,
    ShippingDiscount
}

public enum CouponUsageType
{
    Unlimited = 0,
    NTimesOnly,
    NTimesPerCustomer
}

internal class DiscountMap : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.HasQueryFilter(x => !x.Deleted);
        builder
            .HasMany(x => x.AppliedToProducts)
            .WithMany()
            .UsingEntity("Discount_ProductDiscount_Mapping",
                r => r
                    .HasOne(typeof(Product))
                    .WithMany()
                    .HasForeignKey("ProductId"),
                l => l
                    .HasOne(typeof(Discount))
                    .WithMany()
                    .HasForeignKey("DiscountId"));
        builder
            .HasMany(x => x.AppliedToShipping)
            .WithMany()
            .UsingEntity("Discount_ShippingDiscount_Mapping",
                r => r
                    .HasOne(typeof(Shipping))
                    .WithMany()
                    .HasForeignKey("ShippingId"),
                l => l
                    .HasOne(typeof(Discount))
                    .WithMany()
                    .HasForeignKey("DiscountId"));
    }
}

public class Discount : BaseEntity, IAuditableEntity, ISoftDeletableEntity
{
    [Required] public string Name { get; set; }
    public DiscountType DiscountType { get; set; }
    public bool UsePercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime? StartsOnUtc { get; set; }

    public DateTime? EndsOnUtc { get; set; }

    //TODO: how to keep the state consistent: no requirement - no coupon code can be persisted?
    public bool IsCouponRequired { get; set; }
    [StringLength(100)] public string CouponCode { get; set; }
    public CouponUsageType CouponUsageType { get; set; }
    public int CouponUsageAmount { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
    public bool Deleted { get; set; }
    public DiscountBadge Badge { get; set; }
    public ICollection<Product> AppliedToProducts { get; set; }
    public ICollection<Shipping> AppliedToShipping { get; set; }
}