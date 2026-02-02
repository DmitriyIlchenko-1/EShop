namespace EShop.Infrastructure.Engine.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public abstract class ComponentLifetimeAttribute : Attribute
{
    public Lifetime Lifetime { get; set; }

    public ComponentLifetimeAttribute(Lifetime lifetime)
    {
        Lifetime = lifetime;
    }
}