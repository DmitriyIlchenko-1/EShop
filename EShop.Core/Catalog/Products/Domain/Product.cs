using System.ComponentModel.DataAnnotations;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Content.Media.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Products.Domain;

internal class ProductMap : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasQueryFilter(x => !x.Deleted);
        
        builder
            .HasOne(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.SetNull);
        
    }
}
public class Product : BaseEntity, IAuditableEntity, ISoftDeletedEntity
{
    [Required, StringLength(200)] 
    public string Name { get; set; }
    
    [StringLength(5000)] 
    public string Description { get; set; }
    
    [StringLength(2000)] 
    public string ShortDescription { get; set; }
    [StringLength(400)]
    public string MetaTitle { get; set; }
    [StringLength(400)]
    public string MetaDescription { get; set; }
    [StringLength(400)]
    public string MetaKeywords { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
    public bool Published { get; set; }
    public bool Deleted { get; set; }

    [StringLength(400)]
    public string Sku { get; set; }

    [StringLength(400)]
    public string Gtin { get; set; }

    public bool HasOptions { get; set; }

    public bool IsAllowToOrder { get; set; }

    public bool ShowOnHomePage { get; set; }
    public bool HomePageDisplayOrder { get; set; }

    public bool IsVisibleIndividually { get; set; }

    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal? SpecialPrice { get; set; }

    public DateTime? SpecialPriceEndsUtc { get; set; }

    public DateTime? SpecialPriceStartsUtc { get; set; }

    public int ApprovedRatingSum { get; set; }
    public int NotApprovedRatingSum { get; set; }

    public int ApprovedReviewCount { get; set; }
    public int NotApprovedReviewCount { get; set; }

    public int StockQuantity { get; set; }
    public decimal Height { get; set; }
    public decimal Weight { get; set; }

    public decimal Width { get; set; }
    public decimal Length { get; set; }

    public Brand Brand { get; set; }

    public int? BrandId { get; set; }
    

    public ICollection<ProductReview> ProductReviews { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; } = [];

    public ICollection<ProductLink> ProductLinks { get; set; }

    
    public ICollection<ProductMedia> ProductMedias { get; set; }

    public ICollection<ProductSpecificationAttribute> ProductSpecificationAttributes { get; set; }

    public ICollection<ProductVariantAttributeCombination> ProductVariantAttributeCombinations { get; set; }

    public ICollection<ProductVariantAttribute> ProductVariantAttributes { get; set; }

    public void AddAttributeCombination(ProductVariantAttributeCombination combination)
    {
        combination.Product = this;
        ProductVariantAttributeCombinations.Add(combination);
    }

    public void AddCategory(ProductCategory category)
    {
        category.Product = this;
        ProductCategories.Add(category);
    }

    public void AddMedia(ProductMedia productMedia)
    {
        productMedia.Product = this;
        ProductMedias.Add(productMedia);
    }
}