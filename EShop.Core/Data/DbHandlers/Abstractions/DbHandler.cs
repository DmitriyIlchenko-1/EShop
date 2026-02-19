using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data.DbHandlers.Abstractions;

/// <summary>
/// Abstract base class to inherit for db handler types when the db handler type needs to implement only a subset of the interface methods.
/// Db handler can also inherit this class to receive typed entity references to work with. 
/// </summary>
/// <typeparam name="TEntity">The entity type the db handler works with</typeparam>
public abstract class DbHandler<TEntity> : DbHandler<TEntity, ApplicationDbContext> where TEntity : BaseEntity
{
}

public abstract class DbHandler<TEntity, TContext> : IDbHandler<TContext>
    where TEntity : BaseEntity where TContext : DbContext
{
    private static readonly Type EntityType = typeof(TEntity);

    // a template method
    public async Task<DbHandlerResult> OnSaveChangesExecuting(IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        var typedEntity = (TEntity)entityContext.Entity;

        switch (entityContext.InitialEntityState)
        {
            case EntityState.Added:
                return await OnInsertingAsync(typedEntity, entityContext, cancellationToken);
            case EntityState.Modified:
                return await OnUpdatingAsync(typedEntity, entityContext, cancellationToken);
            case EntityState.Deleted:
                return await OnDeletingAsync(typedEntity, entityContext, cancellationToken);
            default:
                return DbHandlerResult.Void;
        }
    }

    public async Task<DbHandlerResult> OnSaveChangesExecuted(IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        var typedEntity = entityContext.Entity as TEntity;
        switch (entityContext.InitialEntityState)
        {
            case EntityState.Added:
                return await OnInsertedAsync(typedEntity, entityContext, cancellationToken);
            case EntityState.Modified:
                return await OnUpdatedAsync(typedEntity, entityContext, cancellationToken);
            case EntityState.Deleted:
                return await OnDeletedAsync(typedEntity, entityContext, cancellationToken);
            default:
                return DbHandlerResult.Void;
        }
    }

    public async Task OnAllCompletedSaveChangesExecuting(IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default)
    {
        // arrays return a new instance of their enumerator every time to avoid traversing the same one more than once.
        var handleEntityContexts = entityContexts as IHandleEntityContext[] ?? entityContexts.ToArray();
        await OnAllCompletedSaveChangesExecuting(
            handleEntityContexts
                .Select(x => x.Entity)
                .OfType<TEntity>(),
            handleEntityContexts,
            cancellationToken);
    }

    public async Task OnAllCompletedSaveChangesExecuted(IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default)
    {
        // arrays return a new instance of their enumerator every time to avoid traversing the same one more than once.
        var handleEntityContexts = entityContexts as IHandleEntityContext[] ?? entityContexts.ToArray();
        await OnAllCompletedSaveChangesExecuted(
            handleEntityContexts
                .Select(x => x.Entity)
                .OfType<TEntity>(),
            handleEntityContexts,
            cancellationToken);
    }

    protected virtual Task<DbHandlerResult> OnInsertingAsync(TEntity entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
        => Task.FromResult(DbHandlerResult.Void);

    protected virtual Task<DbHandlerResult> OnUpdatingAsync(TEntity entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
        => Task.FromResult(DbHandlerResult.Void);

    protected virtual Task<DbHandlerResult> OnDeletingAsync(TEntity entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
        => Task.FromResult(DbHandlerResult.Void);

    protected virtual Task<DbHandlerResult> OnInsertedAsync(TEntity entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
        => Task.FromResult(DbHandlerResult.Void);

    protected virtual Task<DbHandlerResult> OnUpdatedAsync(TEntity entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
        => Task.FromResult(DbHandlerResult.Void);

    protected virtual Task<DbHandlerResult> OnDeletedAsync(TEntity entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
        => Task.FromResult(DbHandlerResult.Void);

    /// <summary>
    /// This is a hook operation, which does nothing by default.
    /// It may be overriden by a subclass to provide concrete behaviour in case the subclass is interested in handling entity at this stage.
    /// </summary>
    protected virtual Task OnAllCompletedSaveChangesExecuting(IEnumerable<TEntity> entities,
        IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// This is a hook operation, which does nothing by default.
    /// It may be overriden by a subclass to provide concrete behaviour in case the subclass is interested in handling entity at this stage.
    /// </summary>
    protected virtual Task OnAllCompletedSaveChangesExecuted(IEnumerable<TEntity> entities,
        IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}