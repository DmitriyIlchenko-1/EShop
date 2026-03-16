using Autofac;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure;
using EShop.Infrastructure.Data.DbHandlers;
using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Engine.Attributes;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Data.Launch;

public class DbHandlerModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        AddRequiredDependencies(builder);
        ConnectImplementationsWithMetadata(builder);
    }

    private static void AddRequiredDependencies(ContainerBuilder builder)
    {
        builder
            .RegisterType<DefaultDbHandlerActivator>()
            .As<IDbHandlerActivator>()
            .InstancePerDependency();
        builder
            .RegisterType<DefaultDbHandlerDispatcher>()
            .As<IDbHandlerDispatcher>()
            .InstancePerDependency();
        builder
            .RegisterType<DefaultDbHandlerRegistry>()
            .As<IDbHandlerRegistry>()
            .SingleInstance();
    }

    private static void ConnectImplementationsWithMetadata(ContainerBuilder builder)
    {
        var typeScanner = Singleton<ITypeScanner>.Instance;
        var foundHandlers = typeScanner.FindClassesOfType<IDbHandler>(onlyConcreteClasses: true);
        
        foreach (var handlerT in foundHandlers)
        {
            // Find out which entity type the handler works with.
            var (entityT, contextT) = GetHandlerEntityType(handlerT);

            // serviceInterfaces are the interfaces that are going to be used to try to instantiate the db handler. 
            // If a service is also a db handler and is already registered as the impl of its service, then this is one of the ways of resolving the handler from the DI container.
            // If a type just implements the handler interface (IDbHandler) and isn't a service, we add the actual type to the collection to be able to instantiate it later on in the activator class.

            // We want to register this type that implements the IDBHandler interface,
            // but we also wanna expose it as any other service interface it implements
            // The thing is we do not want to have multiple registrations of the same type,
            // because it doesn't make much sense even if it seems harmful (except for memory consumption) and
            // so we want to have one registration that exposes this type as all the interfaces it implements.
            // And so what lets us achieve that is that the initial registrations happen in the ConfigureServices() method of the engine startup type
            // and after that, the ConfigureContainer() method gets called, which is where this module is called from
            // and all the registrations that happen in here override the registrations made in the ConfigureServices().
            // So unlike what we have if we, for example, register the same type multiple times in a row in the ConfigureServices(),
            // in which case we would still have multiple registrations, and the last one would win when we want to resolve the component,
            // in this case, Autofac actually overrides the registrations so we only end up with one registration for this type (component).
            var serviceInterfaces = handlerT
                .GetInterfaces()
                .Where(x => !(x.IsGenericType && x.IsClosedTypeOf(typeof(IDbHandler<>))))
                .Except(GlobalConfiguration.IgnoredInterfaces.Concat([typeof(IDbHandler)])) 
                .ToArray();
            if (!serviceInterfaces.Any())
                serviceInterfaces = new Type[] { handlerT };

            // Register the handler as IDbHandler along with its metadata.
            //In Autofac, implementation type is activated once and could be returned as different service types, whereas for MS DI,
            //implementation could be activated for each service type. This is because RegisterType != Add*
            var registration = builder
                .RegisterType(handlerT)
                .As(typeof(IDbHandler)) // we register a handler as HandlerType purely to retrieve the related metadata later on in DbHandlerRegistry's ctor. 
                .WithMetadata<DbHandlerMetadata>(md =>
                {
                    md.For(x => x.EntityType, entityT);
                    md.For(x => x.ExposedServiceTypes, serviceInterfaces);
                    md.For(x => x.HandlerType, handlerT);
                    md.For(x => x.DbContextType, contextT);
                })
                .InstanceScopeFromAttribute(fallback: Lifetime.InstancePerDependency);

            if (serviceInterfaces.Any())
            {
                registration.As(serviceInterfaces);
            }
            else
            {
                // do not need to do that unless this is the only way to resolve the type (the type implements no other interfaces)
                registration.AsSelf();
            }
        }
    }

    private static (Type Entity, Type DbContext) GetHandlerEntityType(Type dbHandlerType)
    {
        var baseType = dbHandlerType.BaseType;
        while (baseType != null && baseType != typeof(object))
        {
            if (baseType.IsGenericType)
            {
                var openGenericType = baseType.GetGenericTypeDefinition();

                if (openGenericType == typeof(AsyncDbHandler<>))
                {
                    return (baseType
                        .GetGenericArguments()[0], typeof(ApplicationDbContext));
                }

                if (openGenericType == typeof(AsyncDbHandler<,>))
                {
                    return (baseType
                        .GetGenericArguments()[0], baseType
                        .GetGenericArguments()[1]);
                }
            }

            baseType = baseType.BaseType;
        }


        var @interface = dbHandlerType
            .GetInterfaces()
            .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDbHandler<>));
        if (@interface != null)
        {
            return (typeof(BaseEntity), @interface
                .GetGenericArguments()[0]);
        }

        return (typeof(BaseEntity), typeof(ApplicationDbContext));
    }
}