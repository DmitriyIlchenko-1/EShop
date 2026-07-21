using EShop.Infrastructure.Modules;

namespace EShop.Core.Platform.Modules.Payment;

public interface IPaymentMethod : IProvider
{
    
}



internal class TestPaymentMethod : IPaymentMethod
{
    
}