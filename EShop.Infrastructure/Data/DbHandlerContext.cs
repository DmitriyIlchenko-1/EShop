using EShop.Core.Data.DbHandlers;
using EShop.Infrastructure.Engine;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

 

public abstract class DbHandlerContext : DbContext
{
    private readonly Stack<DbSaveChangesOperation> _saveOperations = new Stack<DbSaveChangesOperation>();

    public DbHandlerContext(DbContextOptions options) : base(options)
    {
    }
    
    //todo
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

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
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
            return await currentSaveOperation.ExecuteAsync(acceptAllChangesOnSuccess, cancellationToken);
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
    

    /// <summary>
/// Makes a call to the actual (base) DbContext.SaveChangesAsync() 
/// </summary>
    protected internal Task<int> SaveChangesCoreAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    
    
}