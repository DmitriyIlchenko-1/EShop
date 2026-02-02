namespace EShop.Infrastructure.Engine.Attributes;


public class InstancePerLifetimeScopeAttribute : ComponentLifetimeAttribute
{
    public InstancePerLifetimeScopeAttribute() : base(Attributes.Lifetime.InstancePerLifetimeScope)
    {
    }
}