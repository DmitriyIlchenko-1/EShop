using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EShop.Core.Data.DbHandlers;

/// <summary>
///     A class-wrapper representing a single (including the parent if exists SaveChanges()) operation that can be performed against a database.
///     A new operation gets constructed for each SaveChanges() call.
/// </summary>
internal class DbSaveChangesOperation
{
    private readonly DbHandlerContext _dbContext;
    private EntityEntry[] _changedEntries;
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

    public virtual async Task<int> ExecuteAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
    {
        Exception? exception = null;

        await using (await DoExecute())
        {
            try
            {
                // Make a call to the DbContext.SaveChangesAsync() to actually save changes.
                return await _dbContext.SaveChangesCoreAsync(acceptAllChangesOnSuccess, cancellationToken);
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

                if (result.InvokedDbHandlers.Any() && entries.Any(x => x.EntityState == EntityState.Modified))
                {
                    // todo Why do we detect changes only of the state is modified? If I remove an entity, will it be detected? And if so, why call DetectChanges() in here then?
                    // todo Does EntityState return the accurate value even if automatic change detection is off?? 
                    _dbContext.ChangeTracker.DetectChanges();
                }
            }
        }

        //the inner piece of code executes only if you manually modify the entityState property inside one of the db handlers.
        if (result.AnyStateChanged)
        {
            // todo keep narrowing down the number of entries to only the ones that have been changed in some ways
            // by the db handlers so that we pass to the 'after' methods only the processed entries. 
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

        //todo: change the state instance variable & do the cache invalidation

        return await _dispatcher.SavedChangesInvokeAsync(entries, cancellationToken);
    }

    private static bool CanBeHandled(object instance)
    {
        return instance is BaseEntity;
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

    //todo: dispose? 
}