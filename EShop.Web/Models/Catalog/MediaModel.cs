using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class MediaModel : BaseModel
{
    public string Url { get; set; }
    public string Alt { get; set; }

    public int? Width { get; set; }
    public int? Height { get; set; }
}