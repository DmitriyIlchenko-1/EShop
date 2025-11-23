using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class ProductReviewItemModel : BaseModel
{
    public string Title { get; set; }
    public string CommentText { get; set; }
    public int Rating { get; set; }
    public string ReviewerName { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime EditedOn { get; set; }
    public ICollection<ReplyModel> Replies { get; set; }
}