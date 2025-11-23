using System.ComponentModel.DataAnnotations;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Brands.Domain;

internal class BrandMap : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasQueryFilter(x => !x.Deleted);
    }
}
public class Brand : BaseEntity, IAuditableEntity, ISoftDeletedEntity, IDisplayOrder
{
    [StringLength(200)]
    public string Name { get; set; }
    [StringLength(3000)]
    public string Description { get; set; }

    public bool Deleted { get; set; }
    public bool IsPublished { get; set; }
 
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
    public int DisplayOrder { get; set; }
}