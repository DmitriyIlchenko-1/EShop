using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class ProductSummaryReviewModel : BaseModel
{
    public int ProductId { get; set; }
    public int RatingSum { get; set; }
    public int TotalReviews { get; set; }
    
    //TODO: only if they've bought that product before.
    public bool CanCurrentUserLeaveReview { get; set; }
}