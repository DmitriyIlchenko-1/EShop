using EShop.Infrastructure.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EShop.Core.Data.DbHandlers;

public class SaveChangesExecutingResult : SaveChangesExecutedResult
{
    public SaveChangesExecutingResult(IEnumerable<IDbHandler> processedHandlers, bool anyStateChanged) : base(
        processedHandlers)
    {
        AnyStateChanged = anyStateChanged;
    }

    public new static readonly SaveChangesExecutingResult Empty =
        new SaveChangesExecutingResult(Enumerable.Empty<IDbHandler>(), false);

    public bool AnyStateChanged { get; }
    public IHandleEntityContext[] Entries { get; set; }
}

public class SaveChangesExecutedResult
{
    public SaveChangesExecutedResult(IEnumerable<IDbHandler> processedDbHandlers)
    {
        ProcessedDbHandlers = processedDbHandlers;
    }

    public static readonly SaveChangesExecutedResult Empty
        = new SaveChangesExecutedResult(Enumerable.Empty<IDbHandler>());

    public IEnumerable<IDbHandler> ProcessedDbHandlers { get; }
}

public class DefaultDbHandlerDispatcher : IDbHandlerDispatcher
{
    private readonly IDbHandlerRegistry _registry;
    private readonly IDbHandlerActivator _activator;
    private readonly bool _hasHooks;

    public DefaultDbHandlerDispatcher(IDbHandlerRegistry registry, IDbHandlerActivator activator)
    {
        _registry = registry;
        _activator = activator;
        _hasHooks = registry.AllMetadata.Length > 0;
    }


    //todo 
    public ILogger Logger { get; set; } = NullLogger.Instance;


    public async Task<SaveChangesExecutingResult> SavingChangesInvokeAsync(IHandleEntityContext[] entities,
        CancellationToken cancellationToken = default)
    {
        return (SaveChangesExecutingResult)await InvokeInternal(entities,
            DbHandlerStage.BeforeSaving,
            cancellationToken);
    }

    public async Task<SaveChangesExecutedResult> SavedChangesInvokeAsync(IHandleEntityContext[] entities,
        CancellationToken cancellationToken = default)
    {
        return await InvokeInternal(entities, DbHandlerStage.AfterSaving, cancellationToken);
    }

    private async Task<SaveChangesExecutedResult> InvokeInternal(IHandleEntityContext[] entities, DbHandlerStage stage,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var before = DbHandlerStage.BeforeSaving == stage;
        if (!_hasHooks || entities.Length == 0)
        {
            return before ? SaveChangesExecutingResult.Empty : SaveChangesExecutedResult.Empty;
        }

        var invokedDbHandlers = new MultiMap<IDbHandler, IHandleEntityContext>();
        var anyStateChanged = false;
        TryGetSharedDbHandlers(entities, stage, out var shared);

        foreach (var entity in entities)
        {
            // 
            if (cancellationToken.IsCancellationRequested)
            {
                continue;
            }

            // reuse the ones we have because we already know that they are for the same entity type and the ef state and the stage to that matter.
            var dbHandlers = shared ?? _registry.SelectDbHandlers(entity, stage);

            foreach (var dbHandler in dbHandlers)
            {
                try
                {
                    Logger.LogDebug(
                        $"Executing {dbHandler.GetType().Name}. Stage: {stage}, Entity: {entity.EntityType.Name}, Entity's state: {entity.EntityState}.");

                    var instance = _activator.Activate(dbHandler);
                    //If the result is Void, it means that the handler isn't interested in handling with the given type of Entity (entity.EntityType) and so its OnAll...() isn't called later on.
                    DbHandlerResult result = before
                        ? await instance.OnSaveChangesExecutingAsync(entity, cancellationToken)
                        : await instance.OnSaveChangesExecutedAsync(entity, cancellationToken);

                    if (result == DbHandlerResult.Ok)
                    {
                        invokedDbHandlers.Add(instance, entity);
                    }
                    else if (result == DbHandlerResult.Void)
                    {
                        _registry.RemoveVoidDbHandler(dbHandler, entity, stage);
                    }
                }
                catch (Exception ex) when (ex is NotImplementedException || ex is NotSupportedException)
                {
                    _registry.RemoveVoidDbHandler(dbHandler, entity, stage);
                }
                catch (Exception ex)
                {
                    /* TODO: I don't know if we should swallow exceptions in here.
                      We make a trade-off that the value of continuing to execute the code is higher than the risk of having inconsistencies. */
                    Logger.LogError(ex, $"An exception has been thrown executing {dbHandler.GetType().FullName}.");
                }

                if (before && entity.HasStateChanged)
                {
                    entity.InitialEntityState = entity.EntityState;
                    anyStateChanged = true;
                }
            }
        }

        foreach (var pair in invokedDbHandlers)
        {
            if (before)
            {
                await pair.Key.OnAllCompletedSaveChangesExecutingAsync(pair.Value, cancellationToken);
            }
            else
            {
                await pair.Key.OnAllCompletedSaveChangesExecutedAsync(pair.Value, cancellationToken);
            }
        }

        return before
            ? new SaveChangesExecutingResult(invokedDbHandlers.Keys, anyStateChanged)
            {
                Entries = entities
            }
            : new SaveChangesExecutedResult(invokedDbHandlers.Keys);
    }

    /// <summary>
    /// If all the entities are the same type and have the same state, we don't have to call _registry.SelectDbHandlers() every time in the for loop
    /// and so what we do instead is we try to identify that and get all the db handlers at once and reuse them as we go through the loop later on.
    /// </summary>
    private bool TryGetSharedDbHandlers(IHandleEntityContext[] entities, DbHandlerStage stage,
        out DbHandlerMetadata[] result)
    {
        ArgumentNullException.ThrowIfNull(entities);
        result = null;

        // > 3
        if (!(entities.Length > 1))
            return false;
        var firstEntity = entities[0];
        var entityState = firstEntity.InitialEntityState;
        var entityType = firstEntity.EntityType;
        for (int i = 1; i < entities.Length; i++)
        {
            if (entityType != entities[i].EntityType || entities[i].InitialEntityState != entityState)
            {
                return false;
            }
        }

        result = _registry.SelectDbHandlers(entities[0], stage);
        return true;
    }
}