namespace EShop.Infrastructure.Engine.Attributes;

public enum Lifetime
{
    InstancePerDependency,
    InstancePerLifetimeScope,
    SingleInstance
}