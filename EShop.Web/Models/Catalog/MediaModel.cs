using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class MediaModel : BaseModel
{
    public string FileName { get; set; }
    public string Alt { get; set; }
    public string MimeType { get; set; }
    public string MediaType { get; set; }
    public int Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Url { get; set; }
}