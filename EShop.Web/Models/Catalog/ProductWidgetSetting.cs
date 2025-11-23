 

using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace EShop.Web.Models.Catalog;

public class ProductWidgetSetting
{
    public long? CategoryId { get; set; }

    public bool FeaturedOnly { get; set; }

    public int NumberOfProducts { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ProductWidgetOrderBy OrderBy { get; set; }
}