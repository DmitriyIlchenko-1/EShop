using System.ComponentModel.DataAnnotations;
using EShop.Core.Content.Media.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Categories.Domain;

internal class BrandMap : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasQueryFilter(x => !x.Deleted);
        builder
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict); //TODO: what about 'No action'? Should look into it. 
        
        //If an image has already been uploaded for another entity - we will reuse it.
        //If the image gets removed - set the NP to null.
        builder
            .HasOne(x => x.MediaFile)
            .WithMany()
            .HasForeignKey(x => x.MediaFileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class Category : BaseEntity, IAuditableEntity, ISoftDeletedEntity, IDisplayOrder
{
    [Required, StringLength(200)] public string Name { get; set; }

    [StringLength(500)] public string Description { get; set; }

    //TODO: Do we need to keep it?
    public string Slug { get; set; }

    [StringLength(400)] public string MetaTitle { get; set; }
    [StringLength(400)] public string MetaDescription { get; set; }
    [StringLength(400)] public string MetaKeywords { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
    public bool ShowOnHomePage { get; set; }
    public bool IncludeInMenu { get; set; }
    public bool IsPublished { get; set; }
    public bool IsRootParent { get; set; }
    public bool Deleted { get; set; }
    public int DisplayOrder { get; set; }
    public Category Parent { get; set; }

    public int? ParentId { get; set; }

    public List<Category> Children { get; set; }

    public int? MediaFileId { get; set; }

    public MediaFile MediaFile { get; set; }
}