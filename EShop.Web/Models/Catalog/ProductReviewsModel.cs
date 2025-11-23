using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class ProductReviewsModel : BaseModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int TotalReviewsCount { get; set; }

    public double RatingAverage
        => TotalReviewsCount > 0
            ? ((1 * Rating1Count) + (2 * Rating2Count) + (3 * Rating3Count) + (4 * Rating4Count) +
               (5 * Rating5Count)) / (double)TotalReviewsCount
            : 0;


    public int Rating1Count { get; set; }
    public int Rating2Count { get; set; }
    public int Rating3Count { get; set; }
    public int Rating4Count { get; set; }
    public int Rating5Count { get; set; }

    public ICollection<ProductReviewItemModel> ReviewItems { get; set; }
}