using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data.DbHandlers;

/// <summary>
/// Inspired by: https://learn.microsoft.com/en-us/archive/msdn-magazine/2008/june/patterns-in-practice-the-open-closed-principle#figure-2-introducing-a-chain-of-responsibility.
/// This interface is used mostly as the marker to help to identify db handlers in the IoC container.
/// It isn't recommended to use the interface directly because you'd have to implement the contract even if you don't use some of the methods in here as well as no typed entity reference/es is/are provided.
/// </summary>
public interface IDbHandler
{  
    /// <summary>
    /// This method is called before a call to SaveChanges(Async) for every entity found as the result of a call to EF Core's Change Tracker. 
    /// </summary>
    Task<DbHandlerResult> OnSaveChangesExecutingAsync(IHandleEntityContext entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// This method is called after a call to SaveChanges(Async) happens for every entity found as the result of a call to EF Core's Change Tracker. 
    /// </summary>
    Task<DbHandlerResult> OnSaveChangesExecutedAsync(IHandleEntityContext entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// This method is called before a call to SaveChanges(Async) for every entity found as the result of a call to EF Core's Change Tracker. 
    /// </summary>
    Task OnAllCompletedSaveChangesExecutingAsync(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// This method is called before a call to SaveChanges(Async) for every entity found as the result of a call to EF Core's Change Tracker. 
    /// </summary>
    Task OnAllCompletedSaveChangesExecutedAsync(IEnumerable<IHandleEntityContext> entities,
        CancellationToken cancellationToken = default);
}

public interface IDbHandler<TContext> : IDbHandler where TContext : DbContext
{
    
}