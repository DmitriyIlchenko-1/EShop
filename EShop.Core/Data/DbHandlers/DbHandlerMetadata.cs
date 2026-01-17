namespace EShop.Core.Data.DbHandlers;

public class DbHandlerMetadata
{
    public Type HandlerType { get; set; }
    public Type EntityType { get; set; }
    public IList<Type> ServiceTypes { get; set; }
}