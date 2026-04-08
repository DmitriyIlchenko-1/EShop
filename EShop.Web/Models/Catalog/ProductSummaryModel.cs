using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;
using EShop.Web.Common.Models;
using EShop.Web.Controllers;
using Riok.Mapperly.Abstractions;

namespace EShop.Web.Models.Catalog;


public class ProductSummaryItemContext
{
    public ProductLazyContext LazyProductContext { get; set; }
    public IDictionary<int, ProductVariantAttributeSelection> ProductSelectedAttributes { get; set; }

    public ICollection<ProductVariantAttributeCombination> ProductSelectedCombinations { get; set; }
    public CatalogHelper.ProductSummaryMappingSettings MappingSettings { get; set; }
}

public class ProductSummaryModel : BaseModel, IDisposable
{
    public static readonly ProductSummaryModel Empty = new ProductSummaryModel();
    public List<ProductSummaryItemModel> Items { get; set; } = new();

    public void Dispose()
    {
        Items.Clear();
    }
}

public class ProductSummaryItemModel : BaseModel
{
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public string Sku { get; set; }
    public string SeName { get; set; }
    public string Dimensions { get; set; }
    public int TotalReviews { get; set; }
    public bool IsAvailable { get; set; }
    public int RatingAverage { get; set; }
    public int Quantity { get; set; }
    public bool IsShippingEnabled { get; set; }
    public BrandSummaryModel Brand { get; set; }
    public List<ProductSpecificationModel> Specifications { get; set; }
    public List<ProductVariantAttributeModel> Variants { get; set; }
    public ProductVariantAttributeSelection Selection { get; set; }
    public ProductSummaryPriceModel PriceModel { get; set; } = new();
    public IList<MediaModel> Images { get; set; }
    public ProductSummaryReviewModel ReviewModel { get; set; }
}