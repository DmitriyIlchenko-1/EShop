using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Attributes.Domain;

internal class ProductAttributeOptionsSetMap : IEntityTypeConfiguration<ProductAttributeOptionsSet>
{
    public void Configure(EntityTypeBuilder<ProductAttributeOptionsSet> builder)
    {
        builder
            .HasOne(p => p.ProductAttribute)
            .WithMany(p => p.ProductAttributeOptionsSets)
            .HasForeignKey(p => p.ProductAttributeId);
    }
}

/// <summary>
/// Represents the table with product attribute option sets the options. It has <see cref="ProductAttribute"/> its values belong to.
/// Each set can be copied to  Product's unique attributes. 
/// </summary>
public class ProductAttributeOptionsSet : BaseEntity
{
    public string Name { get; set; }

    public ProductAttribute ProductAttribute { get; set; }

    public int ProductAttributeId { get; set; }

    public ICollection<ProductAttributeOption> ProductAttributeOptions { get; set; }
}