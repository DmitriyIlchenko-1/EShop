using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Checkout.Shipping.Configuration;

public class ShippingSettings : ISettings
{
    public decimal FreeShippingOverXValue { get; set; }
    public bool FreeShippingOverXActive { get; set; }
    public int TodayShipmentHour { get; set; } = 16;
    public bool DeliveryOnWorkweekDaysOnly { get; set; }
}