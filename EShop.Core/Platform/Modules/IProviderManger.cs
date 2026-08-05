using Autofac;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Modules.Payment;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Modules;

namespace EShop.Core.Platform.Modules;

public interface IProviderManager
{
    IEnumerable<Provider<TProvider>> GetProviders<TProvider>() where TProvider : IProvider;
    Provider<TProvider> GetProvider<TProvider>(string systemName) where TProvider : IProvider;
}

public class DefaultProviderManager : IProviderManager
{
    readonly IComponentContext _ctx;

    public DefaultProviderManager(IComponentContext ctx)
    {
        _ctx = ctx;
    }

    public IEnumerable<Provider<TProvider>> GetProviders<TProvider>() where TProvider : IProvider
    {
        var providers = _ctx.Resolve<IEnumerable<Lazy<TProvider, ProviderMetadata>>>();
        return providers.Select(lazy => new Provider<TProvider>(lazy));
    }

    public Provider<TProvider> GetProvider<TProvider>(string systemName) where TProvider : IProvider
    {
        if (systemName.IsEmpty())
        {
            return null;
        }

        var provider = _ctx.ResolveOptionalNamed<Lazy<TProvider, ProviderMetadata>>(systemName);
        return new Provider<TProvider>(provider);
    }
}

public interface IPaymentProviderManager
{
    IEnumerable<Provider<IPaymentMethod>> GetActivePaymentMethods();
    Provider<IPaymentMethod> GetActivePaymentMethod(string systemName);
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
        var allPaymentMethods = _providerManager.GetProviders<IPaymentMethod>().ToArray();
        if (allPaymentMethods.Length == 0)
        {
            throw new App
        }
        return allPaymentMethods;
    }

    public Provider<IPaymentMethod> GetActivePaymentMethod(string systemName)
        => _providerManager.GetProvider<IPaymentMethod>(systemName);
}