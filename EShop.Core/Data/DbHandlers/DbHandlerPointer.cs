namespace EShop.Core.Data.DbHandlers;

public struct DbHandlerPointer
{
    private readonly IDbHandler[] _handlers;
    private int _index;

    public DbHandlerPointer(IDbHandler[] handlers)
    {
        _handlers = handlers;
        _index = 0;
    }

    public void Reset()
    {
        _index = 0;
    }

    public THandler? GetNextHandler<THandler>() where THandler : class, IDbHandler
    {
        while (_index < _handlers.Length)
        {
            var handler = _handlers[_index] as THandler;
            _index += 1;
            if (handler != null)
            {
                return handler;
            }
        }
        
        return null;
    }
}