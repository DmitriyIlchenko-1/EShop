using EShop.Core.Data.DbHandlers;
using EShop.Infrastructure.Data.DbHandlers;
using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Engine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using EfState = Microsoft.EntityFrameworkCore.EntityState;

namespace EShop.Infrastructure.Data;

public abstract class DbHandlerContext : DbContext
{
    private readonly Stack<DbSaveChangesOperation> _saveOperations = new Stack<DbSaveChangesOperation>();

    public DbHandlerContext(DbContextOptions options) : base(options)
    {
        ChangeTracker.Tracked += OnTracked;
        ChangeTracker.StateChanged += OnStateChanged;
    }

    private void OnTracked(object sender, EntityEntryEventArgs e)
    {
        var entry = e.Entry;
        if (entry.Entity is BaseEntity entity && (entry.State == EfState.Unchanged || entry.State == EfState.Modified))
        {
            InjectLazyLoader(entity, entry.Context);
        }
    }

    private void OnStateChanged(object sender, EntityStateChangedEventArgs e)
    {
        var entry = e.Entry;
        if (entry.Entity is BaseEntity entity)
        {
            if (e.NewState == EfState.Unchanged || entry.State == EfState.Modified)
            {
                InjectLazyLoader(entity, entry.Context);
            }
            else
            {
                UnsetLazyLoader(entity);
            }
        }
    }

    private IDbHandlerDispatcher ActivateDbHandlerDispatcher()
    {
        try
        {
            return EngineContext.Current.ResolveOptional<IDbHandlerDispatcher>() ??
                   NullDbHandlerDispatcher.Instance;
        }
        catch
        {
            return NullDbHandlerDispatcher.Instance;
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
        => SaveChangesInternal(acceptAllChangesOnSuccess, false)
            .GetAwaiter()
            .GetResult();

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new CancellationToken())
        => SaveChangesInternal(acceptAllChangesOnSuccess, true, cancellationToken);

    private async Task<int> SaveChangesInternal(bool acceptAllChangesOnSuccess, bool async,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _saveOperations.TryPeek(out var currentSaveOperation);

        if (currentSaveOperation == null)
        {
            currentSaveOperation = new DbSaveChangesOperation(this, ActivateDbHandlerDispatcher());
        }
        else
        {
            if (currentSaveOperation.Stage == DbHandlerStage.BeforeSaving)
            {
                // If all the handlers haven't been called yet, we'll end up making recursive calls if we let this execute normally.
                // We return 0 to cancel the save operation.
                return 0;
            }

            if (currentSaveOperation.Stage == DbHandlerStage.AfterSaving)
            {
                currentSaveOperation = new DbSaveChangesOperation(currentSaveOperation);
            }
        }

        _saveOperations.Push(currentSaveOperation);

        try
        {
            if (async)
            {
                return await currentSaveOperation.ExecuteAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            else
            {
                return currentSaveOperation.Execute(acceptAllChangesOnSuccess);
            }
        }
        finally
        {
            //we've got to take operations off the stack to let non-nested ones execute normally
            //without limiting their method calls to avoid recursion.
            _saveOperations.TryPop(out currentSaveOperation);
            currentSaveOperation?.Dispose();
        }
    }

    public override void Dispose()
    {
        ResetState();
        base.Dispose();
    }

    private void ResetState()
    {
        while (_saveOperations.TryPop(out var operation))
        {
            operation.Dispose();
        }
    }


    protected internal int SaveChangesCore(bool acceptAllChangesOnSuccess)
    {
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// Makes a call to the actual (base) DbContext.SaveChangesAsync() 
    /// </summary>
    protected internal Task<int> SaveChangesCoreAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private static void InjectLazyLoader(BaseEntity entity, DbContext db)
    {
        if (entity.LazyLoader is null)
        {
            var lazyLoader = db.GetService<ILazyLoader>();
            entity.LazyLoader = lazyLoader;
        }
    }

    private static void UnsetLazyLoader(BaseEntity entity)
    {
        if (entity.LazyLoader is LazyLoader lazyLoader)
        {
            lazyLoader.Dispose();
            entity.LazyLoader = null;
        }
    }
}