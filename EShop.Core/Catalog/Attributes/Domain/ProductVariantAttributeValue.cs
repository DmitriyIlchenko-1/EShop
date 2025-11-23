using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Attributes.Domain;

internal class ProductVariantAttributeValueMap : IEntityTypeConfiguration<ProductVariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductVariantAttributeValue> builder)
    {
        builder
            .HasOne(p => p.ProductVariantAttribute)
            .WithMany(p => p.ProductVariantAttributeValues)
            .HasForeignKey(p => p.ProductVariantAttributeId);
    }
}

/// <summary>
/// Represents a value for a product attribute, which is linked to the product via <see cref="ProductVariantAttribute"/>.
/// So every value we copy or create to assign to the Product's attribute is stored in this table. 
/// </summary>
public class ProductVariantAttributeValue : BaseEntity
{
    public string Alias { get; set; }

    public string Color { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsPreSelected { get; set; }

    public string Name { get; set; }

    public decimal PriceAdjustment { get; set; }

    public ProductVariantAttribute ProductVariantAttribute { get; set; }

    public int ProductVariantAttributeId { get; set; }

    public decimal WeightAdjustment { get; set; }
}