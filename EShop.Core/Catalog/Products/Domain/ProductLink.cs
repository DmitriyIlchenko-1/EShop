using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Products.Domain;

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

public class ProductLink : BaseEntity
{
    public int DisplayOrder { get; set; }

    public Product LinkedProduct { get; set; }

    public int LinkedProductId { get; set; }

    public Product Product { get; set; }

    public int ProductId { get; set; }

    //public ProductLinkType ProductLinkType { get; set; }
}