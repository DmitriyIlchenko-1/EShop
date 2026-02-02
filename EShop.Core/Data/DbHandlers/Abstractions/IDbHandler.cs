using EShop.Infrastructure.Domain;

namespace EShop.Core.Data.DbHandlers;

/// <summary>
/// This interface is used mostly as the marker to help to identify db handlers in the IoC container.
/// It isn't recommended to use the interface directly because you'd have to implement the contract even if you don't use some of the methods in here as well as no typed entity reference/es is/are provided. 
/// </summary>
public interface IDbHandler
{
    Task OnSaveChangesExecuting(IHandleEntityContext entity, CancellationToken cancellationToken = default);
    Task OnSaveChangesExecuted(IHandleEntityContext entity, CancellationToken cancellationToken = default);

    Task OnAllCompletedSaveChangesExecuting(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken = default);

    Task OnAllCompletedSaveChangesExecuted(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken = default);
}