using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data.DbHandlers.Abstractions;

namespace EShop.Core.Data.DbHandlers;

public class CacheInvalidationHandler : DbHandler<Product>
{
    protected override async Task OnSaveChangesExecuting(Product entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(9999);
    }
}

public class CacheInvalidationHandler2 : CacheInvalidationHandler
{
    protected override async Task OnSaveChangesExecuting(Product entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(9999);
    }
}