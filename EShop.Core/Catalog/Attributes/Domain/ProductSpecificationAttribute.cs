using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Attributes.Domain;

 

public class ProductSpecificationAttribute : BaseEntity
{
    public int DisplayOrder { get; set; }

    public Product Product { get; set; }

    public int ProductId { get; set; }

    public SpecificationAttributeOption SpecificationAttributeOption { get; set; }

    public int SpecificationAttributeOptionId { get; set; }
}