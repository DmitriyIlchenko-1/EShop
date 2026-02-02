namespace EShop.Infrastructure.Engine.Attributes;


public class SingleInstanceAttribute : ComponentLifetimeAttribute
{
    public SingleInstanceAttribute() : base(Attributes.Lifetime.SingleInstance)
    {
    }
}