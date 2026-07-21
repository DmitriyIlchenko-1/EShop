using System.ComponentModel.DataAnnotations;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Content.Media.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Brands.Domain;

internal class BrandMap : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder
            .HasOne(x => x.MediaFile)
            .WithMany()
            .HasForeignKey(x => x.MediaFileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
public class Brand : BaseEntity, IAuditableEntity, ISoftDeletableEntity, IDisplayOrder
{
    [StringLength(200)]
    public string Name { get; set; }
    [StringLength(3000)]
    public string Description { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPublished { get; set; }
 
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
    public int DisplayOrder { get; set; }
    
    public int? MediaFileId { get; set; }
    public MediaFile MediaFile { get; set; }
    public ICollection<Product> Products { get; set; }
  
}