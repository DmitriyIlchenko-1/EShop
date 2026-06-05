using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class BrandSummaryModel : BaseModel
{
    public string Name { get; set; }
    public string SeName { get; set; }
    public ImageModel Image { get; set; }
}