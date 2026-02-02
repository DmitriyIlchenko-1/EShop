using Autofac.Builder;
using EShop.Infrastructure.Engine.Attributes;

namespace EShop.Infrastructure.Extensions;

public static class AutofacExtensions
{
    public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstanceScopeFromAttribute<TLimit,
        TActivatorData, TRegistrationStyle>(
        this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration,
        Lifetime fallback = Lifetime.InstancePerDependency) where TActivatorData : ReflectionActivatorData
    {
        var lifetimeScope = registration.ActivatorData.ImplementationType
            .GetSingleAttribute<ComponentLifetimeAttribute>(false)
            ?
            .Lifetime ?? fallback;

        switch (lifetimeScope)
        {
            case Lifetime.InstancePerDependency:
                registration.InstancePerDependency();
                break;
            case Lifetime.InstancePerLifetimeScope:
                registration.InstancePerLifetimeScope();
                break;
            case Lifetime.SingleInstance:
                registration.SingleInstance();
                break;
        }

        return registration;
    }
}