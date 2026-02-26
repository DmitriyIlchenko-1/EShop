using System.Collections.Concurrent;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Data.DbHandlers;
using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EShop.Core.Data.DbHandlers;

/// <summary>
///     A class-wrapper representing a single (including the parent if exists SaveChanges()) operation that can be performed against a database.
///     A new operation gets constructed for each SaveChanges() call.
/// </summary>
internal class DbSaveChangesOperation : IDisposable
{
    private DbHandlerContext _dbContext;
    private EntityEntry[] _changedEntries;

    private static readonly ConcurrentDictionary<Type, bool>
        DbHandledEntities = new ConcurrentDictionary<Type, bool>();

    private readonly IDbHandlerDispatcher _dispatcher;
    private readonly bool _isNested;
    public DbHandlerStage Stage { get; private set; }

    public DbSaveChangesOperation(DbHandlerContext dbContext, IDbHandlerDispatcher dispatcher)
    {
        _dbContext = dbContext;
        _dispatcher = dispatcher;
    }

    public DbSaveChangesOperation(DbSaveChangesOperation parent)
    {
        _dbContext = parent._dbContext;
        _dispatcher = parent._dispatcher;
        _isNested = true;
    }

    public int Execute(bool acceptAllChangesOnSuccess)
        => ExecuteInternal(acceptAllChangesOnSuccess, false)
            .GetAwaiter()
            .GetResult();

    public virtual Task<int> ExecuteAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
        => ExecuteInternal(acceptAllChangesOnSuccess, true,cancellationToken);

    private async Task<int> ExecuteInternal(bool acceptAllChangesOnSuccess, bool async, CancellationToken cancellationToken = default)
    {
        Exception exception = null;

        await using (await DoExecute())
        {
            try
            {
                if (async)
                {
                    // Make a call to the DbContext.SaveChangesAsync() to actually save changes.
                    return await _dbContext.SaveChangesCoreAsync(acceptAllChangesOnSuccess, cancellationToken);
                }
                else
                {
                    
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    return _dbContext.SaveChangesCore(acceptAllChangesOnSuccess);
                }
                
            }
            catch (Exception ex)
            {
                // We don't want to swallow the exception by catching a generic exception and not responding to the failure.
                // So what we do is rethrow it to let the outer exception handlers, if present, to take care of it.
                exception = ex;
                throw;
            }
        }


        async Task<IAsyncDisposable> DoExecute()
        {
            var initialAutoDetectChanges = _dbContext.ChangeTracker.AutoDetectChangesEnabled;
            // For performance, we don't want EF Core's internal methods detect changes everytime they get called. We're going to manually detect changes when needed.
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            _dbContext.ChangeTracker.DetectChanges();
            _changedEntries = GetChangedEntities()
                .ToArray();

            var beforeResult = await BeforeSavingExecuteAsync(cancellationToken);

            return new AsyncActionDisposable(EndExecute);

            async ValueTask EndExecute()
            {
                // prevent recursive calls from this side.
                if (_isNested)
                {
                    return;
                }

                try
                {
                    // ReSharper disable once AccessToModifiedClosure
                    if (exception == null)
                    {
                        await AfterSavingExecuteAsync(beforeResult.Entries, cancellationToken);
                    }
                }
                finally
                {
                    // perform necessary clean up whether or not an exception has been throw.
                    _dbContext.ChangeTracker.AutoDetectChangesEnabled = initialAutoDetectChanges;
                }
            }
        }
    }

    private async Task<SaveChangesExecutingResult> BeforeSavingExecuteAsync(CancellationToken cancellationToken)
    {
        var result = SaveChangesExecutingResult.Empty;
        var anyChanged = _changedEntries.Any();
        if (anyChanged)
        {
            var entries = _changedEntries
                .Where(x => CanBeHandled(x.Entity))
                .Select(x => (IHandleEntityContext)new HandleEntityContext(x))
                .ToArray();

            if (entries.Length > 0)
            {
                Stage = DbHandlerStage.BeforeSaving;
                result = await _dispatcher.SavingChangesInvokeAsync(entries, cancellationToken);

                // if there's at least one entity whose state is modified, we trigger a change detection. May seem unintuitive, though,
                // there's no way for an entity to pass this check through if the entity or one of its entities it's in relation with is in any state but Unchanged.
                // In other words, if there's at least one entity whose state is modified, we automatically assume something might have changes so we call DetectChanges().
                if (result.ProcessedDbHandlers.Any() && entries.Any(x => x.EntityState == EntityState.Modified))
                {
                    // we detect changes in here because we want to make sure we step into the SaveChangesAsync() method with up-to-date entries,
                    // because inside the SaveChangesAsync(), the ChangeTracker will not be called and if there's been changes made to the entities in db handlers,
                    // this is the place to discover them.
                    _dbContext.ChangeTracker.DetectChanges();
                }
            }
        }

        //the inner piece of code executes only if you manually modify the entityState property inside one of the db handlers.
        if (result.AnyStateChanged)
        {
            // keep narrowing down the number of entries to only the ones that have been changed in some ways
            // by the db handlers so that we pass to the 'after' methods only the entities that had their changes persisted to the db. 
            result.Entries = result
                .Entries.Where(x => x.EntityState > EntityState.Unchanged)
                .ToArray();
        }

        return result;
    }

    private async Task<SaveChangesExecutedResult> AfterSavingExecuteAsync(IHandleEntityContext[] entries,
        CancellationToken cancellationToken)
    {
        if (entries == null || entries.Length == 0)
        {
            return SaveChangesExecutedResult.Empty;
        }

        Stage = DbHandlerStage.AfterSaving;

        //todo: do the cache invalidation

        return await _dispatcher.SavedChangesInvokeAsync(entries, cancellationToken);
    }

    private static bool CanBeHandled(object instance)
    {
        if (instance is not BaseEntity)
        {
            return false;
        }

        var isHandled = DbHandledEntities.GetOrAdd(instance.GetType(),
            t =>
            {
                var attr = t.GetSingleAttribute<DbHandledAttribute>(true);
                return attr != null ? attr.CanBeHandled : true;
            });

        return isHandled;
    }


    private IEnumerable<EntityEntry> GetChangedEntities()
    {
        var entries = _dbContext.ChangeTracker.Entries();
        foreach (var entry in entries)
        {
            if (entry.State > Microsoft.EntityFrameworkCore.EntityState.Unchanged)
            {
                yield return entry;
            }
        }
    }


    public void Dispose()
    {
        // Assign large managed object references to null to make them more likely to be unreachable. 
        _dbContext = null;
        _changedEntries = null;
    }
}