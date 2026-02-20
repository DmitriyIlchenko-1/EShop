using Autofac;
using Autofac.Core;

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
    public IDbHandler Activate(DbHandlerMetadata metadata)
        => (IDbHandler)Activator.CreateInstance(metadata.HandlerType);
}