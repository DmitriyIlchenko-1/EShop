using System.ComponentModel.DataAnnotations;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Products.Domain;

internal class ReplyMap : IEntityTypeConfiguration<Reply>
{
    public void Configure(EntityTypeBuilder<Reply> builder)
    {
        builder.HasQueryFilter(x => !x.Deleted);
        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
public class Reply : BaseEntity, IAuditableEntity, ISoftDeletedEntity
{
    [StringLength(100)]
    public string ReplierName { get; set; }
    [StringLength(4000)]
    public string ReplyText { get; set; }
    public bool Deleted { get; set; }
    public ReplyStatus ReplyStatus { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public int ProductReviewId { get; set; }
    public ProductReview ProductReview { get; set; }
     
}