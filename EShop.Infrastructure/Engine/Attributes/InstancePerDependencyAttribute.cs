namespace EShop.Infrastructure.Engine.Attributes;


public class InstancePerDependencyAttribute : ComponentLifetimeAttribute
{
    public InstancePerDependencyAttribute() : base(Attributes.Lifetime.InstancePerDependency)
    {
    }
}