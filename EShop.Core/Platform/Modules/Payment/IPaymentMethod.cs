 
using EShop.Core.Checkout.Orders.Services;
using EShop.Infrastructure.Modules;

namespace EShop.Core.Platform.Modules.Payment;

public interface IPaymentMethod : IProvider
{
    Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request);
    public bool SkipPaymentInfo { get; }
}



 

