using Autofac;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Modules.Payment;
using EShop.Infrastructure.Modules;

namespace EShop.Core.Platform.Modules;

public interface IProviderManager
{
    IEnumerable<Provider<TProvider>> GetProviders<TProvider>() where TProvider : IProvider;
}

public class DefaultProviderManager : IProviderManager
{
    readonly IComponentContext _componentContext;

    public DefaultProviderManager(IComponentContext componentContext)
    {
        _componentContext = componentContext;
    }

    public IEnumerable<Provider<TProvider>> GetProviders<TProvider>() where TProvider : IProvider
    {
        var providers = _componentContext.Resolve<IEnumerable<Lazy<TProvider, ProviderMetadata>>>();
        return providers.Select(lazy => new Provider<TProvider>(lazy));
    }
}

public interface IPaymentProviderManager
{
    IEnumerable<Provider<IPaymentMethod>> GetActivePaymentMethods();
}

public class PaymentProviderManager : IPaymentProviderManager
{
    readonly IProviderManager _providerManager;

    public PaymentProviderManager(IProviderManager providerManager)
    {
        _providerManager = providerManager;
    }

    public virtual IEnumerable<Provider<IPaymentMethod>> GetActivePaymentMethods()
    {
        var allPaymentMethods = _providerManager.GetProviders<IPaymentMethod>();
        return allPaymentMethods;
    }
}