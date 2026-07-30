using EShop.Infrastructure.Data;
using EShop.Infrastructure.Data.DbHandlers;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Data.DbHandlers;

public class AuditableDbHandler : AsyncDbHandler<IAuditableEntity>
{
    protected override Task<DbHandlerResult> OnInsertingAsync(IAuditableEntity entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DbHandlerResult.Ok);
    }

    protected override Task<DbHandlerResult> OnUpdatingAsync(IAuditableEntity entity, IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DbHandlerResult.Ok);
    }

    protected override Task OnAllCompletedSaveChangesExecutingAsync(IEnumerable<IAuditableEntity> entities,
        IEnumerable<IHandleEntityContext> entityContexts,
        CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            var context = entityContexts.FirstOrDefault(x => x.Entity == entity);
            if (context.EntityState == EntityState.Added)
            {
                entity.CreatedOnUtc = DateTime.UtcNow;
            }
            else if (context.EntityState == EntityState.Modified)
            {
                entity.ModifiedOnUtc = DateTime.UtcNow;
            }
        }
        return Task.CompletedTask;
    }
}