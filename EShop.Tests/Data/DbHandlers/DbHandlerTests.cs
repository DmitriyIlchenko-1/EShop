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
using NUnit.Framework;
using EntityState = EShop.Infrastructure.Data.EntityState;

namespace EShop.Tests.Data.DbHandlers;

[TestFixture]
public class DbHandlerTests
{
    private ApplicationDbContext _db;
    private IEnumerable<Lazy<IDbHandler, DbHandlerMetadata>> _dbHandlers;
    private IDbHandlerDispatcher _dispatcher;
    private IDbHandlerRegistry _registry;


    [OneTimeSetUp]
    public void SetUp()
    {
        _db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
        _dbHandlers =
        [
            CreateDbHandler<DbHandler_Category_OnSaveBefore, Category>(),
            CreateDbHandler<DbHandler_Product_OnSaveAfter, Product>(),
            CreateDbHandler<DbHandler_Entity_Inserted_Deleted_Update, BaseEntity>(),
            CreateDbHandler<DbHandler_Auditable_Inserting_Updating, IAuditableEntity>(),
            CreateDbHandler<DbHandler_SoftDeletable_Deleting_ChangingState, IAuditableEntity>()
        ];
        _registry = new DefaultDbHandlerRegistry(_dbHandlers);
        _dispatcher = new DefaultDbHandlerDispatcher(_registry, new SimpleDbHandlerActivator());
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task Can_Handle_Voidness()
    {
        var entries = new[]
        {
            // Before: DbHandler_Auditable_Inserting_Updating, DbHandler_Entity_Inserted_Deleted_Update.
            // After: DbHandler_Product_OnSaveAfter.
            CreateContext<Product>(EntityState.Modified),  
            
            // After: DbHandler_Entity_Inserted_Deleted_Update. 
            CreateContext<ProductAttribute>(EntityState.Deleted),
            
            // Before: DbHandler_SoftDeletable_Deleting_ChangingState
            // After: DbHandler_Entity_Inserted_Deleted_Update. 
            CreateContext<ProductReview>(EntityState.Deleted),
            
            // Before: DbHandler_Category_OnSaveBefore, DbHandler_Auditable_Inserting_Updating
            // After: DbHandler_Entity_Inserted_Deleted_Update. 
            CreateContext<Category>(EntityState.Added),
        };
        
        var expected = GetExpectedDbHandlers(entries, before: true);
        var processedHandlers = (await _dispatcher.SavingChangesInvokeAsync(entries)).ProcessedDbHandlers;
        
        processedHandlers
            .Count()
            .ShouldEqual(expected.Count());
        processedHandlers
            .All(x => expected.Contains(x.GetType()))
            .ShouldBeTrue();
    }

    private ICollection<Type> GetExpectedDbHandlers(IEnumerable<IHandleEntityContext> entries, bool before)
    {
        var result = new HashSet<Type>();
        foreach (var entry in entries)
        {
            foreach (var handler in _dbHandlers)
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
                     || (!before && (state == EntityState.Added || state == EntityState.Deleted || state == EntityState.Modified));
            /*
             * not relevant, though important in general
             * 
             *  Even if one of the handlers changes an entity's state and sets it to EntityState.Unchanged,
             * like what soft deletable handler does, the other handlers that handle this entity still run regardless.
             * At the same time, it means that as long as no other handler changes the entity state back to anything
             * other than EntityState.Unchanged, the after method doesn't run.
             */
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