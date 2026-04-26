using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;
using EShop.Web.Common.Models;
using EShop.Web.Controllers;
using Riok.Mapperly.Abstractions;

namespace EShop.Web.Models.Catalog;

public class ProductAttributeVariantContext
{
    // Keys: Product ids.
    public Dictionary<int, ProductVariantAttributeCombination> ProductCombinations { get; set; } = new();
}
public class ProductSummaryItemContext
{ private readonly WeakReference<ProductSummaryModel> _parent;

    public ProductSummaryItemContext(ProductSummaryModel parent)
    {
        _parent = new WeakReference<ProductSummaryModel>(parent);
    }

    public ProductSummaryModel Model
    {
        get
        {
            _parent.TryGetTarget(out var parent);
            return parent;
        }
    }
    public ProductLazyContext LazyProductContext { get; set; }
    public ProductVariantQuery ProductVariantQuery { get; set; }
    
    // Keys: Product ids.
    public Dictionary<int, ProductVariantAttributeSelection> ProductAttributeSelections { get; set; } = new();
    
    // Keys: Brand ids.
    public Dictionary<int, Brand> Brands { get; set; } = new();

     
    public CatalogHelper.ProductSummaryMappingSettings MappingSettings { get; set; }
}

public class ProductSummaryModel : BaseModel, IDisposable
{
    public static readonly ProductSummaryModel Empty = new ProductSummaryModel();
    public List<ProductSummaryItemModel> Items { get; set; } = new();
    public bool ShowBrand { get; set; }
    public bool ShowRating { get; set; }
 

    public void Dispose()
    {
        Items.Clear();
    }
}

public abstract class ProductCombinationMap : BaseModel
{
    public int TotalAttributeCount { get; set; }
    public ProductVariantAttributeCombination SelectedCombination { get; set; }
    public ICollection<ProductVariantAttributeModel> ProductVariantAttributes { get; set; } = [];
    
    public abstract void AdjustForCombinationActive(bool isActive);
    public abstract void AdjustForAttributeValue(ProductVariantAttributeValueModel chosenAttribute);
}
public class ProductSummaryItemModel : ProductCombinationMap
{
    private readonly WeakReference<ProductSummaryModel> _parent;

    public ProductSummaryItemModel(ProductSummaryModel parent)
    {
        _parent = new WeakReference<ProductSummaryModel>(parent);
    }

    public ProductSummaryModel Parent
    {
        get
        {
            _parent.TryGetTarget(out var parent);
            return parent;
        }
    }
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public string Sku { get; set; }
    public string SeName { get; set; }
    public string Dimensions { get; set; }
    public int TotalReviews { get; set; }
    public int RatingSum { get; set; }
    public bool IsAvailable { get; set; }
    
 
    public int StockQuantity { get; set; }
    public bool IsShippingEnabled { get; set; }
    public decimal? Weight { get; set; }
    public BrandSummaryModel Brand { get; set; }
    public List<ProductSpecificationModel> Specifications { get; set; }
    
    public ProductSummaryPriceModel PriceModel { get; set; } = new();
    public IList<MediaModel> Images { get; set; }
    public DeliveryTimeModel DeliveryTimeModel { get; set; }
    public override void AdjustForCombinationActive(bool isActive)
    {
        this.IsAvailable = isActive;
        //StockAvailability = ... 
    }

    public override void AdjustForAttributeValue(ProductVariantAttributeValueModel chosenAttribute)
    {
        this.Weight += chosenAttribute.ProductVariantAttributeValue.WeightAdjustment;
    }
}