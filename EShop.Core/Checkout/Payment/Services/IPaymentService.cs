 
using EShop.Core.Checkout.Orders.Services;
using EShop.Core.Platform.Modules.Payment;

namespace EShop.Core.Data.Payment.Services;

public interface IPaymentService
{
    Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request);
}