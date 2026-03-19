using System.Collections.Concurrent;
using EShop.Core.Data.DbHandlers;
using EShop.Infrastructure.Extensions;

namespace EShop.Infrastructure.Data.DbHandlers;

// We don't follow the template pattern in here because we don't need a way to provide more controlled extensibility.
// There isn't much centralized logic / possible code reuse in here that would otherwise
// need to be separated into a public non-virtual method that calls a protected virtual ..Core() method.
// By making this method 'public virtual' we're OK with giving up control over what happens inside the methods when it gets overriden.
// (the virtual method's interface must be adhered, though).
public interface IDbHandlerRegistry
{
    DbHandlerMetadata[] SelectDbHandlers(IHandleEntityContext context, DbHandlerStage stage);
 
    /// <summary>
    /// If a handler's removed, it is never returned from <see cref="SelectDbHandlers"/> for the given entity type, entity state and the given <see cref="DbHandlerStage"/>.
    /// It's done so that we don't have to unnecessarily instantiate db handlers that never handle entities that are a particular type, in a particular state in the given stage (before/after). 
    /// </summary>
    void RemoveVoidDbHandler(DbHandlerMetadata metadata, IHandleEntityContext context,
        DbHandlerStage stage);

    DbHandlerMetadata[] GetAllMetadata();
}

// This type would've been made internal if I didn't have to register it in the Core project.
// I think it's fine to let the type return arrays rather than read-only collections because 
// this registry isn't supposed to be used outside the framework
// because it's designed and used in the internal impl. of the framework.
// It's not part of the public API.
public class DefaultDbHandlerRegistry : IDbHandlerRegistry
{
    private readonly ConcurrentDictionary<DbHandlerCacheKey, DbHandlerMetadata[]> _cache = [];
    private readonly DbHandlerMetadata[] _metadata;
    public DbHandlerMetadata[] GetAllMetadata() => _metadata;

    public DefaultDbHandlerRegistry(IEnumerable<Lazy<IDbHandler, DbHandlerMetadata>> metadata)
    {
        _metadata = metadata
            .Select(x => x.Metadata)
            .ToArray();
    }
    public virtual DbHandlerMetadata[] SelectDbHandlers(IHandleEntityContext context, DbHandlerStage stage)
    {
        ArgumentNullException.ThrowIfNull(context);
        var entityType = context.EntityType;
        var cacheKey = new DbHandlerCacheKey(entityType, context.InitialEntityState, stage);
        if (_cache.TryGetValue(cacheKey, out var metadata))
        {
            return metadata;
        }

        return _cache[cacheKey] = FilterByEntityType(_metadata, entityType)
            .ToArray();
    }

    public virtual void RemoveVoidDbHandler(DbHandlerMetadata metadata, IHandleEntityContext context,
        DbHandlerStage stage)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(metadata);

        var cacheKey = new DbHandlerCacheKey(context.EntityType, context.InitialEntityState, stage);
        if (_cache.TryGetValue(cacheKey, out var dbHandlers))
        {
            _cache[cacheKey] = dbHandlers
                .Where(x => x != metadata)
                .ToArray();
        }
    }

    private static IEnumerable<DbHandlerMetadata> FilterByEntityType(DbHandlerMetadata[] source, Type entityType)
    {
        return source.Where(x => x.EntityType.IsAssignableFrom(entityType) && entityType.IsConcrete());
    }

    class DbHandlerCacheKey : Tuple<Type, EntityState, DbHandlerStage>
    {
        public DbHandlerCacheKey(Type entityType, EntityState entityState, DbHandlerStage stage) : base(entityType,
            entityState,
            stage)
        {
        }
    }
}