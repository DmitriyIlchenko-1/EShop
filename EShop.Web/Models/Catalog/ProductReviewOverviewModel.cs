using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class ProductReviewOverviewModel : BaseModel
{
    public int TotalReviews { get; set; }
    public int RatingSum { get; set; }
}