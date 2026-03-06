using System.Diagnostics.CodeAnalysis;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data;
using EShop.Core.Data.DbHandlers;
using EShop.Infrastructure.Data.DbHandlers;
using EShop.Infrastructure.Domain;
using EShop.Tests.Framework;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;
using EntityState = EShop.Infrastructure.Data.EntityState;

namespace EShop.Tests.Data.DbHandlers;

[TestFixture]
public class DbHandlerTests
{
    private Mock<IDbHandlerRegistry> _defaultRegistryMock;
    private IDbHandlerRegistry _defaultRegistry;

    [SetUp]
    public void Reset()
    {
        _defaultRegistryMock = new Mock<IDbHandlerRegistry>();
        _defaultRegistry = _defaultRegistryMock.Object;
    }

    private IDbHandlerDispatcher CreateDispatcher(IDbHandlerRegistry registry = null)
        => new DefaultDbHandlerDispatcher(registry ?? _defaultRegistry, new SimpleDbHandlerActivator());

    [Test]
    public async Task SavingChangesInvokeAsync_No_DbHandlers_Registered_Return_Empty_Result()
    {
        _defaultRegistryMock
            .Setup(x => x.GetAllMetadata())
            .Returns(Array.Empty<DbHandlerMetadata>());

        var dispatcher = CreateDispatcher();
        var result = await dispatcher.SavingChangesInvokeAsync([CreateContext<Product>(EntityState.Modified)]);
        result.ShouldEqual(SaveChangesExecutingResult.Empty);
    }

    [Test]
    public async Task SavingChangesInvokeAsync_No_Entities_Passed_Return_Empty_Result()
    {
        _defaultRegistryMock
            .Setup(x => x.GetAllMetadata())
            .Returns([new DbHandlerMetadata()]);
        var dispatcher = CreateDispatcher();
        var result = await dispatcher.SavingChangesInvokeAsync(Array.Empty<IHandleEntityContext>());
        result.ShouldEqual(SaveChangesExecutingResult.Empty);
    }

    [Test]
    public async Task SavingChangesInvokeAsync_Passed_Entity_Array_Is_Null_Throws()
    {
        _defaultRegistryMock
            .Setup(x => x.GetAllMetadata())
            .Returns([new DbHandlerMetadata()]);

        var dispatcher = CreateDispatcher();
        await Should.ThrowAsync<ArgumentNullException>(
            async () => await dispatcher.SavingChangesInvokeAsync((IHandleEntityContext[])null));
    }

    /// <summary>
    /// Make sure that for a given set of entities,
    /// the dispatcher returns the array with db handlers that have successfully handled the entities.
    /// </summary>
    [Test]
    public async Task Ensure_Calls_Right_Handlers_For_Given_Entities_And_Their_State()
    {

        var entities = new[]
        {
            CreateContext<Product>(EntityState.Modified),
            CreateContext<Category>(EntityState.Added),
            CreateContext<Brand>(EntityState.Deleted),
            CreateContext<ProductReview>(EntityState.Deleted)
        };

        var handlers = new[]
        {
            CreateDbHandler<DbHandler_Entity_Insert, BaseEntity>(),
            CreateDbHandler<DbHandler_Entity_Update, BaseEntity>(),
            CreateDbHandler<DbHandler_Brand_Delete, Brand>(),
            CreateDbHandler<DbHandler_ProductAttribute_OnInserted, ProductAttribute>(),
            CreateDbHandler<DbHandler_Category_Inserted, Category>(),
            CreateDbHandler<DbHandler_Category_OnSaveBefore, Category>(),
            CreateDbHandler<DbHandler_Product_OnSaveAfter, Product>(),
            CreateDbHandler<DbHandler_Entity_Inserted_Deleted_Update, BaseEntity>(),
            CreateDbHandler<DbHandler_Auditable_Inserting_Updating, IAuditableEntity>(),
            CreateDbHandler<DbHandler_SoftDeletable_Deleting_ChangingState, IAuditableEntity>()
        };

        var registry = new DefaultDbHandlerRegistry(handlers);
        var dispatcher = CreateDispatcher(registry);

        var expected = GetExpectedDbHandlers(handlers, entities, before: true);
        var result = await dispatcher.SavingChangesInvokeAsync(entities);
        var processedHandlers = result.ProcessedDbHandlers;

        processedHandlers
            .Count()
            .ShouldEqual(expected.Count());
        processedHandlers
            .All(x => expected.Contains(x.GetType()))
            .ShouldBeTrue();

        entities.Length.ShouldEqual(result.Entries.Length);
        entities.ShouldAllBe(x => result.Entries.Contains(x));
    }

    [Test]
    public async Task SavingChangesInvokeAsync_Change_Entity_State_Reflected_In_Result()
    {
        var entities = new[]
        {
            CreateContext<Brand>(EntityState.Deleted)
        };
        var handlers = new[]
        {
            CreateDbHandler<DbHandler_SoftDeletable_Deleting_ChangingState, IAuditableEntity>()
        };
        var registry = new DefaultDbHandlerRegistry(handlers);
        var dispatcher = CreateDispatcher(registry);
        var result = await dispatcher.SavingChangesInvokeAsync(entities);

        result.AnyStateChanged.ShouldBeTrue();
        entities[0]
            .InitialEntityState.ShouldEqual(EntityState.Modified);
        entities[0]
            .EntityState.ShouldEqual(EntityState.Modified);
    }

    [Test]
    public async Task SavingChangesInvokeAsync_Handler_Throws_Generic_Exception_Execution_Continues()
    {
        var entities = new[]
        {
            CreateContext<Brand>(EntityState.Modified),
            CreateContext<Category>(EntityState.Added),
            CreateContext<Product>(EntityState.Deleted)
        };
        var handlers = new[]
        {
            CreateDbHandler<DbHandler_Product_Delete_Throws, Product>(),
            CreateDbHandler<DbHandler_Entity_Insert, BaseEntity>(),
            CreateDbHandler<DbHandler_Entity_Update, BaseEntity>(),
        };

        var registry = new DefaultDbHandlerRegistry(handlers);
        var dispatcher = CreateDispatcher(registry);
        var result = await dispatcher.SavingChangesInvokeAsync(entities.ToArray());
        var processedHandlers = result.ProcessedDbHandlers;
        var expected = GetExpectedDbHandlers(handlers, entities, before: true);

        processedHandlers
            .Count()
            .ShouldEqual(expected.Count());
        processedHandlers
            .All(x => expected.Contains(x.GetType()))
            .ShouldBeTrue();
        result.Entries.Length.ShouldEqual(entities.Length);
    }


    private static ICollection<Type> GetExpectedDbHandlers(IEnumerable<Lazy<IDbHandler, DbHandlerMetadata>> dbHandlers,
        IEnumerable<IHandleEntityContext> entries, bool before)
    {
        var result = new HashSet<Type>();
        foreach (var entry in entries)
        {
            foreach (var handler in dbHandlers)
            {
                if (ShouldHandle(handler.Metadata.HandlerType, entry, before))
                {
                    result.Add(handler.Metadata.HandlerType);
                }
            }
        }

        return result;
    }

    private static bool ShouldHandle(Type dbHandlerType, IHandleEntityContext context, bool before)
    {
        bool result = false;
        var entityT = context.EntityType;
        var state = context.EntityState;

        if (dbHandlerType == typeof(DbHandler_Entity_Insert))
        {
            result = typeof(BaseEntity).IsAssignableFrom(entityT) && state == EntityState.Added;
        }

        if (dbHandlerType == typeof(DbHandler_Entity_Update))
        {
            result = typeof(BaseEntity).IsAssignableFrom(entityT) && state == EntityState.Modified;
        }

        if (dbHandlerType == typeof(DbHandler_ProductAttribute_OnInserted))
        {
            result = !before && typeof(ProductAttribute).IsAssignableFrom(entityT) && state == EntityState.Added;
        }

        if (dbHandlerType == typeof(DbHandler_Category_Inserted))
        {
            result = !before && typeof(Category).IsAssignableFrom(entityT) && state == EntityState.Modified;
        }

        if (dbHandlerType == typeof(DbHandler_Brand_Delete))
        {
            result = typeof(Brand).IsAssignableFrom(entityT) && state == EntityState.Deleted;
        }

        if (dbHandlerType == typeof(DbHandler_Product_OnSaveAfter))
        {
            result = !before && typeof(Product).IsAssignableFrom(entityT);
        }
        else if (dbHandlerType == typeof(DbHandler_Category_OnSaveBefore))
        {
            result = before && typeof(Category).IsAssignableFrom(entityT);
        }
        else if (dbHandlerType == typeof(DbHandler_Auditable_Inserting_Updating))
        {
            result = before && typeof(IAuditableEntity).IsAssignableFrom(entityT)
                            && (state == EntityState.Added || state == EntityState.Modified);
        }
        else if (dbHandlerType == typeof(DbHandler_Entity_Inserted_Deleted_Update))
        {
            result = (before && (state == EntityState.Modified))
                     || (!before && (state == EntityState.Added || state == EntityState.Deleted ||
                                     state == EntityState.Modified));
        }
        else if (dbHandlerType == typeof(DbHandler_SoftDeletable_Deleting_ChangingState))
        {
            result = before && typeof(ISoftDeletableEntity).IsAssignableFrom(entityT) &&
                     state == EntityState.Deleted;
        }


        return result;
    }

    private static IHandleEntityContext CreateContext<T>(EShop.Infrastructure.Data.EntityState entityState)
        where T : BaseEntity, new() //new() - otherwise can't call the parameterless new T()
    {
        return new HandleEntityContextMock(new T(), entityState);
    }

    private static Lazy<IDbHandler, DbHandlerMetadata> CreateDbHandler<TDbHandler, TEntity>()
        where TDbHandler : IDbHandler, new() where TEntity : class
    {
        return new Lazy<IDbHandler, DbHandlerMetadata>(() => new TDbHandler(),
            new DbHandlerMetadata
            {
                EntityType = typeof(TEntity),
                HandlerType = typeof(TDbHandler),
            });
    }
}