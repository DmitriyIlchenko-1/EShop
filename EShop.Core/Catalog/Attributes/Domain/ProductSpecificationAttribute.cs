using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Attributes.Domain;

internal class ProductLinkMap : IEntityTypeConfiguration<ProductLink>
{
    public void Configure(EntityTypeBuilder<ProductLink> builder)
    {
        builder
            .HasOne(p => p.Product)
            .WithMany(p => p.ProductLinks)
            .HasForeignKey(p => p.ProductId);
        builder
            .HasOne(p => p.LinkedProduct)
            .WithMany()
            .HasForeignKey(p => p.LinkedProductId);
    }
}

public class ProductSpecificationAttribute : BaseEntity
{
    public int DisplayOrder { get; set; }

    public Product Product { get; set; }

    public int ProductId { get; set; }

    public SpecificationAttributeOption SpecificationAttributeOption { get; set; }

    public int SpecificationAttributeOptionId { get; set; }
}