using EShop.Core.Catalog.Products.Domain;

namespace EShop.Core.Data.DbHandlers;

public class CacheInvalidationHandler : IDbHandler<Product>
{
    public async Task OnSaveChangesExecuting(Product entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task OnSaveChangesExecuted(Product entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task OnAllCompletedSaveChangesExecuting(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task OnAllCompletedSaveChangesExecuted(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}