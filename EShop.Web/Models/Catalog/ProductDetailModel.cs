using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Catalog.Products.Services;
using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class ProductDetailModel : BaseModel
{
    public string MetaDescriptions { get; set; }
    public string MetaTitle { get; set; }
    public Money FinalPrice { get; set; }
    public PriceSaving Saving { get; set; }
    public Money RegularPrice { get; set; }
    public string StockAvailability { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ShortDescription { get; set; }
    public int? MaxAddToCartNumber { get; set; }
    public string Sku { get; set; }
    public string Gtin { get; set; }
    public string Weight { get; set; }
    public string Height { get; set; }
    public string Length { get; set; }
    public string Width { get; set; }

    public decimal WeightValue { get; set; }
    public decimal HeightValue { get; set; }
    public decimal LengthValue { get; set; }
    public decimal WidthValue { get; set; }

    public bool IsAvailable { get; set; }
    public double? RatingAverage { get; set; }
    
    public int StockQuantity { get; set; }
    public string DeliveryTimeDate { get; set; }
    public string UpdateUrl { get; set; }
    public ProductVariantAttributeCombination SelectedCombination { get; set; }
    public ICollection<ProductVariantAttributeModel> ProductVariantAttributes { get; set; } = [];
    public ICollection<ProductLabelModel> Labels { get; set; } = [];
    public BrandSummaryModel Brand { get; set; }
    public ProductReviewOverviewModel ProductReviewOverview { get; set; } = new ();
     public ICollection<ImageModel> Images { get; set; } = [];
    public ICollection<ProductSpecificationModel> ProductSpecifications { get; set; } = [];
    public IEnumerable<ProductDetailCategoryModel> Categories { get; set; } = [];
    public ICollection<ProductThumbnail> RelatedProducts { get; set; } = [];

   
}