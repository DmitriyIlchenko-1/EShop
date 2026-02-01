using EShop.Core.Data.DbHandlers.Configuration;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Core.Data.DbHandlers;

public static class ServiceRegister
{
    private static int _registrationTimeout;
    private static readonly Type HandlerType = typeof(IDbHandler<>);

    public static void SetTypeLookupLimitations(DbHandlerServiceConfiguration configuration)
    {
        _registrationTimeout = configuration.Timeout;
    }

    public static void AddDbHandlerClassesWithTimeout(IServiceCollection services,
        DbHandlerServiceConfiguration configuration)
    {
        using (var cts = new CancellationTokenSource(_registrationTimeout))
        {
            try
            {
                AddDbHandlerClasses(services, configuration, cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Timeout waiting for service registration");
            }
        }
    }

    public static void AddDbHandlerClasses(IServiceCollection services, DbHandlerServiceConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
    }

    private static void ConnectImplementationsWithMetadata(IServiceCollection collection,
        CancellationToken cancellationToken = default)
    {
        var typeScanner = Singleton<ITypeScanner>.Instance;
        var assemblies = typeScanner.GetAssemblies();
        var types = assemblies
            .SelectMany(x => x.DefinedTypes)
            .Where(t => t.IsConcrete() && t
                .FindCloseInterfacesOf(HandlerType)
                .Any())
            .Select(t => t.AsType())
            .ToList();

        foreach (var handlerT in types)
        {
            // Find out which entity type the handler works with.
            var entityT = GetHandlerEntityType(handlerT);

            // Register the handler as the service types it implements.
            var serviceInterfaces = handlerT
                .GetInterfaces()
                .Where(t => t != HandlerType)
                .ToList();

            foreach (var serviceInterface in serviceInterfaces)
            {
                collection.AddTransient(serviceInterface, handlerT);
            }

            // Register the handler as IDbHandler along with its metadata.
            var metadata = new DbHandlerMetadata();
            metadata.HandlerType = handlerT;
            metadata.EntityType = entityT;
            metadata.ServiceTypes = serviceInterfaces;

            collection.AddTransient(HandlerType, handlerT);
            collection.AddSingleton(metadata);
        }
    }

    private static Type GetHandlerEntityType(Type dbHandlerType)
    {
        var interfaces = dbHandlerType.FindCloseInterfacesOf(HandlerType);
        foreach (var @interface in interfaces)
        {
            if (@interface.ClosedGenericOf(HandlerType))
            {
                var typeArgument = @interface
                    .GetGenericArguments()[0];
                if (typeArgument.IsAssignableTo(typeof(BaseEntity)))
                {
                    return typeArgument;
                }
            }
        }

        throw new NotImplementedException();
    }
}