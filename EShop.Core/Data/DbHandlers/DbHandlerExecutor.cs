using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data.DbHandlers;

public class DbHandlerExecutor
{
    private readonly DbHandlerContext _db;
    private readonly IDbHandlerInvoker _handlerInvoker;
    private readonly IDbHandlerCollector _collector;
    private readonly IEnumerable<IDbHandlerProvider> _dbHandlerProviders;

    public DbHandlerExecutor(IEnumerable<IDbHandlerProvider> dbHandlerProviders, IDbHandlerCollector collector, IDbHandlerInvoker handlerInvoker, DbHandlerContext db)
    {
        _dbHandlerProviders = dbHandlerProviders;
        _collector = collector;
        _handlerInvoker = handlerInvoker;
        _db = db;
    }

    public async Task ExecuteHandlersAsync()
    {
        
    }
}