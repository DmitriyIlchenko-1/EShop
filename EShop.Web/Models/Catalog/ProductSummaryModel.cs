using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class ProductSummaryModel : BaseModel
{
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public string Sku { get; set; }
    public string SeName { get; set; }
   
    public string Dimensions { get; set; }
    public int TotalReviews { get; set; }
    public bool IsShippingEnabled { get; set; }

    public ProductSummaryPriceModel PriceModel { get; set; } = new();
    public IList<MediaModel> Images { get; set; }
    public ProductSummaryReviewModel ReviewModel { get; set; }

}