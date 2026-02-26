using Autofac;
using Autofac.Core;
using EShop.Infrastructure.Extensions;

namespace EShop.Core.Data.DbHandlers;

public interface IDbHandlerActivator
{
    IDbHandler Activate(DbHandlerMetadata metadata);
}

/// <summary>
/// For unit testing
/// </summary>
internal class SimpleDbHandlerActivator : IDbHandlerActivator
{
    private readonly Dictionary<DbHandlerMetadata, IDbHandler> _cache 
        = new Dictionary<DbHandlerMetadata, IDbHandler>();

    public IDbHandler Activate(DbHandlerMetadata metadata)
        => _cache.GetOrAdd(metadata, _ => (IDbHandler)Activator.CreateInstance(metadata.HandlerType));
}