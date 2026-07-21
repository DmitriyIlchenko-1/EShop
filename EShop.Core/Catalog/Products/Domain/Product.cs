using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Common.Domain;
using EShop.Core.Content.Media.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Products.Domain;

internal class ProductMap : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder
            .HasOne(x => x.Brand)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany<MediaFile>(x => x.MediaFiles)
            .WithMany()
            .UsingEntity<ProductMedia>();
        
        builder.HasMany<Label>(x => x.Labels)
            .WithMany()
            .UsingEntity<ProductLabel>();

        
    }
}

public class Product : BaseEntity, IAuditableEntity, ISoftDeletableEntity, IMergedData
{
    [Required, StringLength(200)] public string Name { get; set; }

    [StringLength(5000)] public string Description { get; set; }

    [StringLength(2000)] public string ShortDescription { get; set; }
    [StringLength(400)] public string MetaTitle { get; set; }
    [StringLength(400)] public string MetaDescription { get; set; }
    [StringLength(400)] public string MetaKeywords { get; set; }
    public int MaxAddToCartNumber { get; set; }
    public int MinAddToCartNumber { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
    public bool HasDiscountsApplied { get; set; }

    private decimal? _basePriceAmount;

    public decimal? BasePriceAmount
    {
        get => this.GetMergedData(nameof(BasePriceAmount), _basePriceAmount);
        set => _basePriceAmount = value;
    }

    private decimal? _basePriceBaseAmount;

    public decimal? BasePriceBaseAmount
    {
        get => this.GetMergedData(nameof(BasePriceBaseAmount), _basePriceBaseAmount);
        set => _basePriceBaseAmount = value;
    }

    public CombinationDisplayBehaviour CombinationDisplayBehaviour { get; set; }

    public bool IsAvailable { get; set; }
    public bool AttributeCombinationRequired { get; set; }
    public bool DisplayStockQuantity { get; set; }
    public bool IsPublished { get; set; }
    public bool IsDeleted { get; set; }
    private int? _deliveryTimeId;

    public int? DeliveryTimeId
    {
        get => this.GetMergedData(nameof(DeliveryTimeId), _deliveryTimeId);
        set => _deliveryTimeId = value;
    }

    private string _sku;

    [StringLength(400)]
    public string Sku
    {
        get => this.GetMergedData(nameof(Sku), _sku);
        set => _sku = value;
    }

    private int _quantityUnitId;

    public int QuantityUnitId
    {
        get => this.GetMergedData(nameof(QuantityUnitId), _quantityUnitId);
        set => _quantityUnitId = value;
    }

    private string _gtin;

    [StringLength(400)]
    public string Gtin
    {
        get => this.GetMergedData(nameof(Gtin), _gtin);
        set => _gtin = value;
    }

    public bool HasOptions { get; set; }


    public bool ShowOnHomePage { get; set; }
    public bool HomePageDisplayOrder { get; set; }

    public bool IsVisibleIndividually { get; set; }
    public bool IsShippingEnabled { get; set; }


    private decimal _price;

    public decimal Price
    {
        get => this.GetMergedData(nameof(Price), _price);
        set => _price = value;
    }

    

    public int ApprovedRatingSum { get; set; }
    public int NotApprovedRatingSum { get; set; }

    public int ApprovedReviewCount { get; set; }
    public int NotApprovedReviewCount { get; set; }

    private int _stockQuantity;

    public int StockQuantity
    {
        get => this.GetMergedData(nameof(StockQuantity), _stockQuantity);
        set => _stockQuantity = value;
    }

    private decimal  _height;

    public decimal  Height
    {
        get => this.GetMergedData(nameof(Height), _height);
        set => _height = value;
    }

    private decimal  _weight;

    public decimal  Weight
    {
        get => this.GetMergedData(nameof(Weight), _weight);
        set => _weight = value;
    }

    private decimal _width;

    public decimal Width
    {
        get => this.GetMergedData(nameof(Width), _width);
        set => _width = value;
    }

    private decimal  _length;

    public decimal  Length
    {
        get => this.GetMergedData(nameof(Length), _length);
        set => _length = value;
    }


    public Brand Brand { get; set; }

    public int? BrandId { get; set; }


    public ICollection<ProductReview> ProductReviews { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; } = [];

    public ICollection<ProductLink> ProductLinks { get; set; }


    public ICollection<MediaFile> MediaFiles { get; set; }

    public ICollection<ProductSpecificationAttribute> ProductSpecificationAttributes { get; set; }

    public ICollection<ProductVariantAttributeCombination> ProductVariantAttributeCombinations { get; set; }

    public ICollection<ProductVariantAttribute> ProductVariantAttributes { get; set; }
    public ICollection<Label> Labels { get; set; } = [];
    public ICollection<Discount> AppliedDiscounts { get; set; } = [];

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

   

    [NotMapped, IgnoreDataMember] public bool IgnoreMerge { get; set; }
    [NotMapped, IgnoreDataMember] public Dictionary<string, object> MergedData { get; set; }
}