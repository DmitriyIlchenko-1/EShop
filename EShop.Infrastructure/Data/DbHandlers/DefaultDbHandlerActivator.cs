using System.Collections.Concurrent;
using Autofac;
using Autofac.Core;
using EShop.Core.Data.DbHandlers;

namespace EShop.Infrastructure.Data.DbHandlers;

public class DefaultDbHandlerActivator : IDbHandlerActivator
{
    private readonly ILifetimeScope _lifetimeScope;
    // We cache instances because some db handlers may hold state that they share across different 'DbHandler' stages (before/after).
    // That is, we wanna make sure the same instance handles each method for each state.
    // It looks better because when you think of each save operation, you think of each one as a unit of work.
    private readonly ConcurrentDictionary<DbHandlerMetadata, IDbHandler> _cache = new();

    public DefaultDbHandlerActivator(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }
    
    // I don't think I should make this one virtual because I see no real scenario for which I would need to be able to extend this method. 
    public IDbHandler Activate(DbHandlerMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        
        if (_cache.TryGetValue(metadata, out var dbHandler))
        {
            return dbHandler;
        }

        foreach (var serviceType in metadata.ExposedServiceTypes)
        {
            if (_lifetimeScope.TryResolve(serviceType, out var instance) && instance is IDbHandler typed)
            {
                dbHandler = _cache[metadata] = typed;
                break;
            }
        }

        if (dbHandler == null)
        {
            throw new DependencyResolutionException($"{metadata.HandlerType.FullName} hasn't been registered");
        }

        return dbHandler;
    }
}