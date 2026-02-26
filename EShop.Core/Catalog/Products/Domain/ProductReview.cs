using System.ComponentModel.DataAnnotations;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Products.Domain;

internal class ProductReviewMap : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.HasQueryFilter(x => !x.Deleted);
        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasMany(x => x.Replies)
            .WithOne(x => x.ProductReview)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductReview : BaseEntity, IAuditableEntity, ISoftDeletableEntity
{
    public ProductReview()
    {
        CreatedOnUtc = DateTime.UtcNow;
        ReviewStatus = ReviewStatus.Pending;
    }

    [StringLength(100)] public string Title { get; set; }
    [StringLength(4000)] public string CommentText { get; set; }
    public int Rating { get; set; }

    [StringLength(100)] public string ReviewerName { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
    public bool Deleted { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public ICollection<Reply> Replies { get; set; }
}