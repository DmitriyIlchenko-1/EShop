using System.ComponentModel.DataAnnotations.Schema;
using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Attributes.Domain;

internal class ProductVariantAttributeMap : IEntityTypeConfiguration<ProductVariantAttribute>
{
    public void Configure(EntityTypeBuilder<ProductVariantAttribute> builder)
    {
        builder
            .HasOne(p =>
                p.ProductAttribute)
            .WithMany()
            .HasForeignKey(p => p.ProductAttributeId);
        builder.ToTable("Catalog_ProductAttribute_Mapping");
    }
}

/// <summary>
/// It is the linking table between a Product and a ProductAttribute.
/// The attribute values for EACH <see cref="ProductAttribute"/> this product is linked to (via this table) are stored in <see cref="ProductVariantAttributeValues"/>
/// In other words when we either copy a set of values lined to a <see cref="ProductAttributeOptionsSet"/>
/// or create a new value for a certain <see cref="ProductAttribute"/>, they are all stored in <see cref="ProductVariantAttributeValue"/> table
/// where each value in this table is linked to a row in this table. 
/// </summary>
public class ProductVariantAttribute : BaseEntity
{
    [NotMapped]
    public AttributeControlType AttributeControlType
    {
        get => (AttributeControlType)AttributeControlTypeId;
        set => AttributeControlTypeId = (int)value;
    }

    public int AttributeControlTypeId { get; set; }

    public int DisplayOrder { get; set; }
    
    public bool IsRequired { get; set; }

    public Product Product { get; set; }

    public ProductAttribute ProductAttribute { get; set; }

    public int ProductAttributeId { get; set; }

    public int ProductId { get; set; }

    public ICollection<ProductVariantAttributeValue> ProductVariantAttributeValues { get; set; }

    public bool IsListTypeAttribute()
    {
        return AttributeControlType switch
        {
            AttributeControlType.RadioList => true,
            _ => false
        };
    }
}