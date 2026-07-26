using EShop.Core.Data.Settings;
using EShop.Core.Platform.Modules.Payment;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Data.Payment;

public static class PaymentExtensionMethods
{
    public static bool IsPaymentMethodActive(this Provider<IPaymentMethod> provider, PaymentSettings paymentSettings)
    {
        Guard.NotNull(provider);
        Guard.NotNull(paymentSettings);
        return paymentSettings.ActivePaymentMethodSystemNames.Contains(provider.Metadata.SystemName,
            StringComparer.InvariantCultureIgnoreCase);
    }
}