using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Attributes.Domain;

internal class SpecificationAttributeOptionMap : IEntityTypeConfiguration<SpecificationAttributeOption>
{
    public void Configure(EntityTypeBuilder<SpecificationAttributeOption> builder)
    {
        builder
            .HasOne(p => p.SpecificationAttribute)
            .WithMany(p => p.SpecificationAttributeOptions)
            .HasForeignKey(p => p.SpecificationAttributeId);
    }
}

public class SpecificationAttributeOption : BaseEntity
{
    public string Color { get; set; }

    public int DisplayOrder { get; set; }

    public string Name { get; set; }

    public int NumberValue { get; set; }

    public ICollection<ProductSpecificationAttribute> ProductSpecificationAttributes { get; set; }

    public SpecificationAttribute SpecificationAttribute { get; set; }

    public int SpecificationAttributeId { get; set; }
}