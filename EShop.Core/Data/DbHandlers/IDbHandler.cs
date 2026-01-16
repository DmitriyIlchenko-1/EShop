using EShop.Infrastructure.Domain;

namespace EShop.Core.Data.DbHandlers;

public interface IDbHandler
{
}

public interface IDbHandler<TEntity> : IDbHandler where TEntity : BaseEntity
{
    Task OnSaveChangesExecuting(TEntity entity, CancellationToken cancellationToken = default);
    Task OnSaveChangesExecuted(TEntity entity, CancellationToken cancellationToken = default);

    Task OnAllCompletedSaveChangesExecuting(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);

    Task OnAllCompletedSaveChangesExecuted(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);
}