using Autofac;
using Autofac.Builder;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Core.Platform.Modules;
using EShop.Core.Platform.Modules.Payment;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;

namespace EShop.Infrastructure.Modules.Launch;

public class ModuleDiscoveryModule : Module
{
    private readonly IApplicationContext _applicationContext;

    public ModuleDiscoveryModule(IApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterType<DefaultProviderManager>()
            .As<IProviderManager>()
            .InstancePerLifetimeScope();

        builder
            .RegisterType<PaymentProviderManager>()
            .As<IPaymentProviderManager>()
            .InstancePerLifetimeScope();

        var providerTypes = Singleton<ITypeScanner>.Instance;
        
        foreach (var providerType in providerTypes.FindClassesOfType<IProvider>())
        {
            var moduleDescriptor = _applicationContext.ModuleCollection.GetModuleByAssembly(providerType.Assembly);
            var systemName = GetSystemName(providerType, moduleDescriptor);
            var friendlyName = GetFriendlyName(providerType, moduleDescriptor);
            var registration = builder.RegisterType(providerType)
                .Named<IProvider>(systemName)
                .InstanceScopeFromAttribute()
                .WithMetadata<ProviderMetadata>(m =>
                {
                    m.For(x => x.SystemName, systemName);
                    m.For(x => x.FriendlyName, friendlyName);
                    m.For(x => x.ModuleDescriptor, moduleDescriptor);
                });
            RegisterProviderAs<IPaymentMethod>(providerType, systemName, registration);
        }
    }

    private static void RegisterProviderAs<TProvider>(Type implType, string systemName,
        IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> builder) where TProvider : IProvider
    {
        if (typeof(TProvider).IsAssignableFrom(implType))
        {
            try
            {
                builder.As<TProvider>().Named<TProvider>(systemName);
            }
            catch
            {
            }
        }
    }
    
    private static string GetSystemName(Type providerType, IModuleDescriptor descriptor)
    {
        if (descriptor != null)
        {
            return descriptor.SystemName;
        }

        return providerType.FullName;
    }
    private static string GetFriendlyName(Type providerType, IModuleDescriptor descriptor)
    {
        if (descriptor != null)
        {
            return descriptor.FriendlyName;
        }

        return providerType.Name;
    }
}