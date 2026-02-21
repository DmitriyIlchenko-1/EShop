using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Data;
using EShop.Core.Data.DbHandlers;
using EShop.Core.Data.DbHandlers.Abstractions;
using EShop.Infrastructure.Data;


public interface IProductServiceTest
{
}

public class ProductServiceTestHandler : AsyncDbHandler<Product>, IProductServiceTest
{
    private readonly ApplicationDbContext _dbContext;

    public ProductServiceTestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // protected override Task<DbHandlerResult> OnInsertingAsync(Product entity, IHandleEntityContext entityContext,
    //     CancellationToken cancellationToken = default)
    // {
    //     Console.WriteLine(entity.Name);
    //     entity.Name = entity.Name + "DbHandlerPrefix(add)";
    //     return Task.FromResult(DbHandlerResult.Ok);
    // }
    //
    // protected override Task<DbHandlerResult> OnUpdatingAsync(Product entity, IHandleEntityContext entityContext,
    //     CancellationToken cancellationToken = default)
    // {
    //     Console.WriteLine(entity.Name);
    //     entity.Name = entity.Name + "DbHandlerPrefix(upd)";
    //     entity.ProductCategories.First()
    //         .Category = null;
    //
    //     return Task.FromResult(DbHandlerResult.Ok);
    // }

    protected override Task<DbHandlerResult> OnDeletingAsync(Product entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        entity.Deleted = true;
        entityContext.EntityState = EntityState.Modified;
        return Task.FromResult(DbHandlerResult.Ok);
    }


    protected override async Task<DbHandlerResult> OnUpdatedAsync(Product entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        var isDeleted = entity.Deleted;
        Console.WriteLine(isDeleted);
        return DbHandlerResult.Ok;
    }
}