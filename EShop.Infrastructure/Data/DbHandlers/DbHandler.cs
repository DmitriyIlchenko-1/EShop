using EShop.Core.Data.DbHandlers;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Data.DbHandlers;

public abstract class DbHandler<TEntity, TContext> : IDbHandler<TContext>
    where TEntity : class where TContext : DbContext
{
    #region Explicit interface impl

    //otherwise there would be no way for use to make IDbHandler.OnSaveChangesExecutingAsync,IDbHandler.OnSaveChangesExecutedAsync etc virtual. 
    Task<DbHandlerResult> IDbHandler.OnSaveChangesExecutingAsync(IHandleEntityContext entityContext,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(OnSaveChangesExecuting(entityContext));
    }

    Task<DbHandlerResult> IDbHandler.OnSaveChangesExecutedAsync(IHandleEntityContext entity,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(OnSaveChangesExecuted(entity));
    }

    Task IDbHandler.OnAllCompletedSaveChangesExecutingAsync(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken)
    {
        OnAllCompletedSaveChangesExecuting(entities);
        return Task.CompletedTask;
    }

    Task IDbHandler.OnAllCompletedSaveChangesExecutedAsync(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken)
    {
        OnAllCompletedSaveChangesExecuted(entities);
        return Task.CompletedTask;
    }

    #endregion


    // a template method
    protected virtual DbHandlerResult OnSaveChangesExecuting(IHandleEntityContext entityContext)
    {
        // without knowledge of T's actual type, the compiler is concerned that the intent in here is to do a custom conversion.
        // The simplest solution is to use the as operator.
        var typedEntity =  entityContext.Entity as TEntity;

        switch (entityContext.InitialEntityState)
        {
            case EntityState.Added:
                return OnInserting(typedEntity, entityContext);
            case EntityState.Modified:
                return OnUpdating(typedEntity, entityContext);
            case EntityState.Deleted:
                return OnDeleting(typedEntity, entityContext);
            default:
                return DbHandlerResult.Void;
        }
    }

    protected virtual DbHandlerResult OnSaveChangesExecuted(IHandleEntityContext entityContext)
    {
        var typedEntity = entityContext.Entity as TEntity;
        switch (entityContext.InitialEntityState)
        {
            case EntityState.Added:
                return OnInserted(typedEntity, entityContext);
            case EntityState.Modified:
                return OnUpdated(typedEntity, entityContext);
            case EntityState.Deleted:
                return OnDeleted(typedEntity, entityContext);
            default:
                return DbHandlerResult.Void;
        }
    }

    protected virtual void OnAllCompletedSaveChangesExecuting(IEnumerable<IHandleEntityContext> entityContexts)
    {
        // arrays return a new instance of their enumerator every time to avoid traversing the same one more than once.
        var handleEntityContexts = entityContexts as IHandleEntityContext[] ?? entityContexts.ToArray();
        OnAllCompletedSaveChangesExecuting(handleEntityContexts
                .Select(x => x.Entity)
                .OfType<TEntity>(),
            handleEntityContexts);
    }

    protected virtual void OnAllCompletedSaveChangesExecuted(IEnumerable<IHandleEntityContext> entityContexts)
    {
        // arrays return a new instance of their enumerator every time to avoid traversing the same one more than once.
        var handleEntityContexts = entityContexts as IHandleEntityContext[] ?? entityContexts.ToArray();
        OnAllCompletedSaveChangesExecuted(handleEntityContexts
                .Select(x => x.Entity)
                .OfType<TEntity>(),
            handleEntityContexts);
    }

    protected virtual DbHandlerResult OnInserting(TEntity entity, IHandleEntityContext entityContext) =>
        DbHandlerResult.Void;

    protected virtual DbHandlerResult OnUpdating(TEntity entity, IHandleEntityContext entityContext) =>
        DbHandlerResult.Void;

    protected virtual DbHandlerResult OnDeleting(TEntity entity, IHandleEntityContext entityContext) =>
        DbHandlerResult.Void;

    protected virtual DbHandlerResult OnInserted(TEntity entity, IHandleEntityContext entityContext) =>
        DbHandlerResult.Void;

    protected virtual DbHandlerResult OnUpdated(TEntity entity, IHandleEntityContext entityContext) =>
        DbHandlerResult.Void;

    protected virtual DbHandlerResult OnDeleted(TEntity entity, IHandleEntityContext entityContext) =>
        DbHandlerResult.Void;

    /// <summary>
    /// This is a hook operation, which does nothing by default.
    /// It may be overriden by a subclass to provide concrete behaviour in case the subclass is interested in handling entities at this stage.
    /// </summary>
    protected virtual void OnAllCompletedSaveChangesExecuting(IEnumerable<TEntity> entities,
        IEnumerable<IHandleEntityContext> entityContexts)
    {
    }

    /// <summary>
    /// This is a hook operation, which does nothing by default.
    /// It may be overriden by a subclass to provide concrete behaviour in case the subclass is interested in handling entities at this stage.
    /// </summary>
    protected virtual void OnAllCompletedSaveChangesExecuted(IEnumerable<TEntity> entities,
        IEnumerable<IHandleEntityContext> entityContexts)
    {
    }
}