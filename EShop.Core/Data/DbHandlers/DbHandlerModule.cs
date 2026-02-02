using System.Reflection;
using Autofac;
using EShop.Core.Data.DbHandlers.Configuration;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Core.Data.DbHandlers;

public class DbHandlerModule : Autofac.Module
{
    private readonly DbHandlerComponentConfiguration _componentConfiguration;
    private static readonly Type HandlerType = typeof(IDbHandler<>);

    public DbHandlerModule(Action<DbHandlerComponentConfiguration> configAction)
    {
        var configuration = new DbHandlerComponentConfiguration();
        configAction(configuration);
        _componentConfiguration = configuration;
    }

    protected override void Load(ContainerBuilder builder)
    {
        AddDbHandlerClassesWithTimeout(builder, _componentConfiguration);
    }

    public static void AddDbHandlerClassesWithTimeout(ContainerBuilder builder,
        DbHandlerComponentConfiguration configuration)
    {
        using (var cts = new CancellationTokenSource(configuration.Timeout))
        {
            try
            {
                AddDbHandlerClasses(builder, configuration, cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Timeout waiting for service registration");
            }
        }
    }

    private static void AddDbHandlerClasses(ContainerBuilder builder, DbHandlerComponentConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var typeScanner = Singleton<ITypeScanner>.Instance;
        var assemblies = typeScanner.GetAssemblies();
        ConnectImplementationsWithMetadata(builder, assemblies, cancellationToken);
    }

    private static void ConnectImplementationsWithMetadata(ContainerBuilder builder, IEnumerable<Assembly> assemblies,
        CancellationToken cancellationToken = default)
    {
        var foundHandlers = assemblies
            .SelectMany(x => x.DefinedTypes)
            .Where(t => t.IsConcrete() && t
                .FindCloseInterfacesOf(HandlerType)
                .Any())
            .Select(t => t.AsType())
            .ToList();

        foreach (var handlerT in foundHandlers)
        {
            // Find out which entity type the handler works with.
            var entityT = GetHandlerEntityType(handlerT);

            // Register the handler as the service types it implements.
            var serviceInterfaces = handlerT
                .GetInterfaces()
                .Where(t => t != HandlerType)
                .ToArray();

            foreach (var serviceInterface in serviceInterfaces)
            {
                builder
                    .RegisterType(handlerT)
                    .As(serviceInterface)
                    .InstancePerDependency();
            }

            // Register the handler as IDbHandler along with its metadata.
            builder
                .RegisterType(handlerT)
                .As(HandlerType)
                .WithMetadata<DbHandlerMetadata>(md =>
                {
                    md.For(x => x.EntityType, entityT);
                    md.For(x => x.ServiceTypes, serviceInterfaces);
                    md.For(x => x.HandlerType, handlerT);
                })
                .InstancePerDependency();
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

        throw new InvalidOperationException(
            $"DbHandler type {dbHandlerType.FullName} doesn't implement {HandlerType.FullName}.");
    }
}