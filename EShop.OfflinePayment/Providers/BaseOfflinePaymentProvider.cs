
using EShop.Core.Checkout.Orders.Services;
using EShop.Core.Platform.Modules.Payment;

namespace EShop.OfflinePayment.Providers;

public abstract class BaseOfflinePaymentProvider : IPaymentMethod
{
    public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request)
        => Task.FromResult(new ProcessPaymentResult(PaymentStatus.Pending));
}