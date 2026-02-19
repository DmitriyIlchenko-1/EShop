using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Data.DbHandlers;
using EShop.Core.Data.DbHandlers.Abstractions;

public interface IProductServiceTest
{
}

public class ProductServiceTestHandler : DbHandler<Product>, IProductServiceTest
{
    protected override Task<DbHandlerResult> OnInsertingAsync(Product entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(entity.Name);
        entity.Name = entity.Name + "DbHandlerPrefix(add)";
        return Task.FromResult(DbHandlerResult.Ok);
    }

    protected override Task<DbHandlerResult> OnUpdatingAsync(Product entity, IHandleEntityContext entityContext, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(entity.Name);
        entity.Name = entity.Name + "DbHandlerPrefix(upd)";
        entity.ProductCategories.First()
            .Category = null;
        return Task.FromResult(DbHandlerResult.Ok);
    }
}