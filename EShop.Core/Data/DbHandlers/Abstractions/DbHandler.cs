using EShop.Infrastructure.Domain;

namespace EShop.Core.Data.DbHandlers.Abstractions;

/// <summary>
/// Abstract base class to inherit for db handler types when the db handler type needs to implement only a subset of the interface methods.
/// Db handler can also inherit this class to receive typed entity references to work with. 
/// </summary>
/// <typeparam name="TEntity">The entity type the db handler works with</typeparam>
public abstract class DbHandler<TEntity> : IDbHandler where TEntity : BaseEntity
{
    private static readonly Type EntityType = typeof(TEntity);

    public Task OnSaveChangesExecuting(IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        var typedEntity = entityContext.Entity as TEntity;
        if (typedEntity != null)
        {
            return Task.FromResult(OnSaveChangesExecuting(typedEntity, entityContext, cancellationToken));
        }

        return Task.CompletedTask;
    }

    public Task OnSaveChangesExecuted(IHandleEntityContext entityContext, CancellationToken cancellationToken = default)
    {
        var typedEntity = entityContext.Entity as TEntity;
        if (typedEntity != null)
        {
            return Task.FromResult(OnSaveChangesExecuted(typedEntity, entityContext, cancellationToken));
        }

        return Task.CompletedTask;
    }

    public Task OnAllCompletedSaveChangesExecuting(IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default)
    {
        var handleEntityContexts = entityContexts as IHandleEntityContext[] ?? entityContexts.ToArray();
        return Task.FromResult(OnAllCompletedSaveChangesExecuting(handleEntityContexts
                .ToArray()
                .Select(x => x.Entity as TEntity)
                .Where(x => x != null),
            handleEntityContexts,
            cancellationToken));
    }

    public Task OnAllCompletedSaveChangesExecuted(IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default)
    {
        var handleEntityContexts = entityContexts as IHandleEntityContext[] ?? entityContexts.ToArray();
        return Task.FromResult(OnAllCompletedSaveChangesExecuted(handleEntityContexts
                .ToArray()
                .Select(x => x.Entity as TEntity)
                .Where(x => x != null),
            handleEntityContexts,
            cancellationToken));
    }

    /// <summary>
    /// This is a hook operation, which does nothing by default.
    /// It may be overriden by a subclass to provide concrete behaviour in case the subclass is interested in handling entity at this stage.
    /// </summary>
    protected virtual Task OnSaveChangesExecuting(TEntity entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// This is a hook operation, which does nothing by default.
    /// It may be overriden by a subclass to provide concrete behaviour in case the subclass is interested in handling entity at this stage.
    /// </summary>
    protected virtual Task OnSaveChangesExecuted(TEntity entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// This is a hook operation, which does nothing by default.
    /// It may be overriden by a subclass to provide concrete behaviour in case the subclass is interested in handling entity at this stage.
    /// </summary>
    protected virtual Task OnAllCompletedSaveChangesExecuting(IEnumerable<TEntity> entities,
        IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// This is a hook operation, which does nothing by default.
    /// It may be overriden by a subclass to provide concrete behaviour in case the subclass is interested in handling entity at this stage.
    /// </summary>
    protected virtual Task OnAllCompletedSaveChangesExecuted(IEnumerable<TEntity> entities,
        IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}