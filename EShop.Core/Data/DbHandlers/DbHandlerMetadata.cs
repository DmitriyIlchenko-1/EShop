namespace EShop.Core.Data.DbHandlers;

public class  DbHandlerMetadata
{
    public Type HandlerType { get; set; }
    public Type EntityType { get; set; }
    public Type DbContextType { get; set; }
    public IList<Type> ExposedServiceTypes { get; set; }
}