using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Platform.Caching;
using EShop.Infrastructure.Data.DbHandlers;

namespace EShop.Web.Infrastructure.DbHandlers;

public class ModelCacheInvalidator : IDbHandler
{
    // {0}: current user's roles.
    public const string CategoryHomePageModelKey = "pres.category.homepage-{0}";
    public const string CategoryHomePagePatternKey = "pres.category.homepage";

    private readonly ICacheManager _cache;

    public ModelCacheInvalidator(ICacheManager cache)
    {
        _cache = cache;
    }

    public Task<DbHandlerResult> OnSaveChangesExecutingAsync(IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
        => Task.FromResult(DbHandlerResult.Void);

    public async Task<DbHandlerResult> OnSaveChangesExecutedAsync(IHandleEntityContext entityContext,
        CancellationToken cancellationToken = default)
    {
        var state = entityContext.InitialEntityState;
        var entity = entityContext.Entity;
        var result = DbHandlerResult.Ok;


        return result;
    }

    public Task OnAllCompletedSaveChangesExecutingAsync(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task OnAllCompletedSaveChangesExecutedAsync(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}