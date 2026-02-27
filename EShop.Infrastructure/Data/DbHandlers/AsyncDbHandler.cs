using EShop.Core.Data.DbHandlers;
using Microsoft.EntityFrameworkCore;


namespace EShop.Infrastructure.Data.DbHandlers;

public abstract class AsyncDbHandler<TEntity, TContext> : IDbHandler<TContext>
    where TEntity : class where TContext : DbContext
{
    public virtual async Task<DbHandlerResult> OnSaveChangesExecutingAsync(IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        var typedEntity = entityContext.Entity as TEntity;

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

    public virtual async Task<DbHandlerResult> OnSaveChangesExecutedAsync(IHandleEntityContext entityContext,
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

    public virtual async Task OnAllCompletedSaveChangesExecutingAsync(IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default)
    {
        // arrays return a new instance of their enumerator every time to avoid traversing the same one more than once.
        var handleEntityContexts = entityContexts as IHandleEntityContext[] ?? entityContexts.ToArray();
        await OnAllCompletedSaveChangesExecutingAsync(
            handleEntityContexts
                .Select(x => x.Entity)
                .OfType<TEntity>(),
            handleEntityContexts,
            cancellationToken);
    }

    public virtual async Task OnAllCompletedSaveChangesExecutedAsync(IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default)
    {
        // arrays return a new instance of their enumerator every time to avoid traversing the same one more than once.
        var handleEntityContexts = entityContexts as IHandleEntityContext[] ?? entityContexts.ToArray();
        await OnAllCompletedSaveChangesExecutedAsync(
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
    protected virtual Task OnAllCompletedSaveChangesExecutingAsync(IEnumerable<TEntity> entities,
        IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// This is a hook operation, which does nothing by default.
    /// It may be overriden by a subclass to provide concrete behaviour in case the subclass is interested in handling entity at this stage.
    /// </summary>
    protected virtual Task OnAllCompletedSaveChangesExecutedAsync(IEnumerable<TEntity> entities,
        IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}