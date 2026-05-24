using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class CategorySummaryModel : BaseModel
{
    public string Name { get; set; }
    public string Url { get; set; }
    public ImageModel Image { get; set; }
}