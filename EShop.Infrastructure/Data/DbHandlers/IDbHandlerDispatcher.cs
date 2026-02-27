namespace EShop.Infrastructure.Data.DbHandlers;

/// <summary>
/// Determines and dispatches the handlers that need to be called before/after SaveChanges().
/// Before calling the handlers, the dispatcher package the event-related information into a context.
/// The information for the context is provided by the system core.
/// Dispatcher then calls handlers passing them the context with relevant information.
/// </summary>
public interface IDbHandlerDispatcher
{
    Task<SaveChangesExecutingResult> SavingChangesInvokeAsync(IHandleEntityContext[] entities,
        CancellationToken cancellationToken = default);

    Task<SaveChangesExecutedResult> SavedChangesInvokeAsync(IHandleEntityContext[] entities,
        CancellationToken cancellationToken = default);
}


public sealed class NullDbHandlerDispatcher : IDbHandlerDispatcher
{
    public static NullDbHandlerDispatcher Instance { get; } = new NullDbHandlerDispatcher();
    public   Task<SaveChangesExecutingResult> SavingChangesInvokeAsync(IHandleEntityContext[] entities, CancellationToken cancellationToken)
    {
        return Task.FromResult(SaveChangesExecutingResult.Empty);
    }
 
    public  Task<SaveChangesExecutedResult> SavedChangesInvokeAsync(IHandleEntityContext[] entities, CancellationToken cancellationToken)
    {
        return Task.FromResult(SaveChangesExecutedResult.Empty);
    }
}