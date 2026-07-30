using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Data.Settings;

public class PaymentSettings : ISettings
{
    public List<string> ActivePaymentMethodSystemNames { get; set; } = ["EShop.OfflinePayment"];
}