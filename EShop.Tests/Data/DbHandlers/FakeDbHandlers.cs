using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data;
using EShop.Core.Data.DbHandlers;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Data.DbHandlers;
using EShop.Infrastructure.Domain;

// ReSharper disable InconsistentNaming

namespace EShop.Tests.Data.DbHandlers;

// DbHandler_EntityType_OnInserted_OnRemoved_..._OnUpdate(OnUpdating & OnUpdated) 

internal class DbHandler_Entity_Insert : DbHandler<BaseEntity, ApplicationDbContext>
{
    protected override DbHandlerResult OnInserting(BaseEntity entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;

    protected override DbHandlerResult OnInserted(BaseEntity entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;
}

internal class DbHandler_Entity_Update : DbHandler<BaseEntity, ApplicationDbContext>
{
    protected override DbHandlerResult OnUpdating(BaseEntity entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;

    protected override DbHandlerResult OnUpdated(BaseEntity entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;
}

internal class DbHandler_Category_Inserted : DbHandler<Category, ApplicationDbContext>
{
    protected override DbHandlerResult OnInserted(Category entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;
}

internal class DbHandler_Brand_Delete : DbHandler<Brand, ApplicationDbContext>
{
    protected override DbHandlerResult OnDeleting(Brand entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;

    protected override DbHandlerResult OnDeleted(Brand entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;
}

internal class DbHandler_Entity_Inserted_Deleted_Update : DbHandler<BaseEntity, ApplicationDbContext>
{
    protected override DbHandlerResult OnInserted(BaseEntity entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;

    protected override DbHandlerResult OnDeleted(BaseEntity entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;

    protected override DbHandlerResult OnUpdating(BaseEntity entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;

    protected override DbHandlerResult OnUpdated(BaseEntity entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;
}

internal class DbHandler_Auditable_Inserting_Updating : DbHandler<IAuditableEntity, ApplicationDbContext>
{
    protected override DbHandlerResult OnInserting(IAuditableEntity entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;

    protected override DbHandlerResult OnUpdating(IAuditableEntity entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;
}

internal class DbHandler_Product_Delete_Throws : DbHandler<Product>
{
    protected override DbHandlerResult OnDeleting(Product entity, IHandleEntityContext entityContext)
        => throw new DivideByZeroException();

    protected override DbHandlerResult OnDeleted(Product entity, IHandleEntityContext entityContext)
        => throw new DivideByZeroException();
}

//reused in other unit tests
internal class DbHandler_SoftDeletable_Deleting_ChangingState : DbHandler<ISoftDeletableEntity>
{
    protected override DbHandlerResult OnDeleting(ISoftDeletableEntity entity, IHandleEntityContext entityContext)
    {
        entity.IsDeleted = true;
        entityContext.EntityState = EntityState.Modified;
        return DbHandlerResult.Ok;
    }
}

internal class DbHandler_ProductAttribute_OnInserted : DbHandler<ProductAttribute>
{
    protected override DbHandlerResult OnInserted(ProductAttribute entity, IHandleEntityContext entityContext)
        => DbHandlerResult.Ok;
}

internal class DbHandler_Category_OnSaveBefore : IDbHandler
{
    public Task<DbHandlerResult> OnSaveChangesExecutingAsync(IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        if (entityContext.EntityType != typeof(Category))
        {
            return Task.FromResult(DbHandlerResult.Void);
        }

        return Task.FromResult(DbHandlerResult.Ok);
    }

    public Task<DbHandlerResult> OnSaveChangesExecutedAsync(IHandleEntityContext entity,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task OnAllCompletedSaveChangesExecutingAsync(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnAllCompletedSaveChangesExecutedAsync(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

internal class DbHandler_Product_OnSaveAfter : IDbHandler
{
    public Task<DbHandlerResult> OnSaveChangesExecutingAsync(IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<DbHandlerResult> OnSaveChangesExecutedAsync(IHandleEntityContext entity,
        CancellationToken cancellationToken = default)
    {
        if (entity.EntityType != typeof(Product))
        {
            return Task.FromResult(DbHandlerResult.Void);
        }

        return Task.FromResult(DbHandlerResult.Ok);
    }

    public Task OnAllCompletedSaveChangesExecutingAsync(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnAllCompletedSaveChangesExecutedAsync(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}