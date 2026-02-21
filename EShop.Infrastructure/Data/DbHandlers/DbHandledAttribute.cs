namespace EShop.Infrastructure.Data.DbHandlers;

 
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class DbHandledAttribute : Attribute
{
    public DbHandledAttribute(bool canBeHandled)
    {
        CanBeHandled = canBeHandled;
    }

    internal bool CanBeHandled { get; }
}