using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class CategorySummaryModel : BaseModel
{
    public string Name { get; set; }
    public string Url { get; set; }
    public MediaModel Image { get; set; }
}