 
using EShop.Core.Checkout.Orders.Services;
using EShop.Core.Data.Settings;
using EShop.Core.Platform.Modules;
using EShop.Core.Platform.Modules.Payment;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Data.Payment.Services;

public class PaymentService : IPaymentService
{
    private readonly IProviderManager _providerManager;
    private readonly PaymentSettings _paymentSettings;

    public PaymentService(IProviderManager providerManager, PaymentSettings paymentSettings)
    {
        _providerManager = providerManager;
        _paymentSettings = paymentSettings;
    }

    public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        Guard.NotNull(request);
        var paymentMethod = _providerManager.GetProvider<IPaymentMethod>(request.PaymentMethodSystemName)
                            ?? throw new InvalidOperationException("Specified payment method not found");

        if (!paymentMethod.IsPaymentMethodActive(_paymentSettings))
        {
            throw new InvalidOperationException("Chosen payment method is not active");
        }

        return await paymentMethod.Proviver.ProcessPaymentAsync(request);
    }
}