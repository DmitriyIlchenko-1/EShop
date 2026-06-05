using EShop.Web.Common.Models;

namespace EShop.Web.Models.Catalog;

public class DeliveryTimeModel : BaseModel
{
    public bool ShowDeliveryTime { get; set; }
    public string DeliveryTimeName { get; set; }
    public string DeliveryTimeDate { get; set; }
    public string StatusLabel { get; set; }
    public string StockAvailability { get; set; }
}