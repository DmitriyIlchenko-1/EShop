using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Attributes.Domain;

internal class ProductAttributeOptionMap : IEntityTypeConfiguration<ProductAttributeOption>
{
    public void Configure(EntityTypeBuilder<ProductAttributeOption> builder)
    {
        builder
            .HasOne<ProductAttributeOptionsSet>((ProductAttributeOption p) =>
                p.ProductAttributeOptionsSet)
            .WithMany(p => p.ProductAttributeOptions)
            .HasForeignKey(p => p.ProductAttributeOptionsSetId);
    }
}

/// <summary>
/// Represent the table for product attribute options that are part of a <see cref="ProductAttributeOptionsSet"/>
/// attribute set that is shared with (be copied to) any <see cref="Product"/> entity as a set. 
/// </summary>
public class ProductAttributeOption : BaseEntity
{
    public string Alias { get; set; }

    public string Color { get; set; }

    public int DisplayOrder { get; set; }

    public string Name { get; set; }

    public decimal PriceAdjustment { get; set; }

    public ProductAttributeOptionsSet ProductAttributeOptionsSet { get; set; }

    public int ProductAttributeOptionsSetId { get; set; }

    public decimal WeightAdjustment { get; set; }
}