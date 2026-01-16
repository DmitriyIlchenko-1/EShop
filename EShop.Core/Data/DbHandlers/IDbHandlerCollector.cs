namespace EShop.Core.Data.DbHandlers;


public interface IDbHandlerCollector
{
    
}
public class DefaultDbHandlerCollector : IDbHandlerCollector
{
    private readonly IEnumerable<DbHandlerMetadata> _handlerMetadatas;

    public DefaultDbHandlerCollector(IEnumerable<DbHandlerMetadata> handlerMetadatas)
    {
        _handlerMetadatas = handlerMetadatas;
    }


    public IEnumerable<DbHandlerMetadata> GetAllHandlerMetadata()
    => _handlerMetadatas;
    
    
}