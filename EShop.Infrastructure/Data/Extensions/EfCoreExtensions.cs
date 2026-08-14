using System.Linq.Expressions;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using EfState = Microsoft.EntityFrameworkCore.EntityState;

namespace EShop.Infrastructure.Data;

[SuppressMessage("Usage",
    "EF1001:Internal EF Core API usage.",
    Justification = "How else am I supposed to retrieve DbContext from IQueryable?")]
public static class EfCoreExtensions
{
    private readonly static MethodInfo StringSubstringMethod =
        typeof(string).GetMethod(nameof(string.Substring), [typeof(int), typeof(int)]);

    #region EF Core Reflection to speed up performance

    private readonly static FieldInfo _queryCompilerFiledInfo =
        typeof(EntityQueryProvider).GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance);

    private readonly static FieldInfo _queryContextFactoryFieldInfo =
        typeof(QueryCompiler).GetField("_queryContextFactory", BindingFlags.NonPublic | BindingFlags.Instance);

    private readonly static PropertyInfo _relationalDependenciesPropInfo =
        typeof(RelationalQueryContextFactory).GetProperty("Dependencies",
            BindingFlags.NonPublic | BindingFlags.Instance);

    private readonly static PropertyInfo _stateManagerPropInfo =
        typeof(QueryContextDependencies).GetProperty("StateManager",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    #endregion

    private readonly static ConcurrentDictionary<Type, LambdaExpression> _cachedExpressions = [];

    /// <summary>
    /// <see href="https://stackoverflow.com/a/53340563"/>
    /// </summary>
    public static DbContext GetDbContext(this IQueryable query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var queryCompiler = _queryCompilerFiledInfo.GetValue(query.Provider);
        var queryContextFactory = _queryContextFactoryFieldInfo.GetValue(queryCompiler);
        var dependencies = queryContextFactory is RelationalQueryContextFactory
            ? _relationalDependenciesPropInfo.GetValue(queryContextFactory)
            : queryContextFactory
                .GetType()
                .GetProperty("Dependencies", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(queryContextFactory); //For unit testing. 
        var stateManager = (IStateManager)_stateManagerPropInfo.GetValue(dependencies);
        return stateManager.Context;
    }


    public static IQueryable<T> SelectSummaryOnly<T>(this IQueryable<T> query) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(query);
        var selector = GetEntityForSummarySelector<T>(query.GetDbContext()
            .Model);
        if (selector != null)
        {
            return query.Select(selector);
        }

        return query;
    }

    private static Expression<Func<T, T>> GetEntityForSummarySelector<T>(IModel entityModel) where T : BaseEntity
    {
        // represents the lambda expression parameter x.
        ParameterExpression lambdaParamNode = Expression.Parameter(typeof(T), "x");

        LambdaExpression selectExpression = _cachedExpressions.GetOrAdd(typeof(T),
            key =>
            {
                // new T();
                NewExpression newEntityExpression = Expression.New(typeof(T));
                List<MemberBinding> memberBindings = new List<MemberBinding>();
                int nonSummaryAttributeCount = 0;
                var entityType = entityModel.FindEntityType(typeof(T));

                //TODO: Find a more efficient way to do that
                var props = typeof(T).GetProperties();
                foreach (var prop in props)
                {
                    if (prop.IsDefined(typeof(NotMappedAttribute), true))
                    {
                        continue;
                    }

                    if (!prop.CanWrite && prop.GetSetMethod(false) != null)
                    {
                        continue;
                    }

                    var nonSummaryAttribute = prop.GetSingleAttribute<NonSummaryAttribute>(true);
                    if (nonSummaryAttribute != null)
                    {
                        nonSummaryAttributeCount++;

                        //Unless the PropertyType is string, or MaxLength isn't null - we skip this property like we're supposed to.
                        if (nonSummaryAttribute.MaxLength == null || prop.PropertyType != typeof(string))
                        {
                            continue;
                        }
                    }

                    if (!prop.PropertyType.IsBasicOrNullableType())
                    {
                        if (entityType
                                .FindProperty(prop)
                                ?.GetValueConverter() == null)
                        {
                            continue;
                        }
                    }

                    // captures accessing each property: x.prop; 
                    MemberExpression propAccessNode = Expression.Property(lambdaParamNode, prop);
                    Expression valueToAssignNode = propAccessNode;

                    if (nonSummaryAttribute?.MaxLength != null)
                    {
                        // captures the call to string.Substring(): x.prop.Substring(0, MaxLength.Value);
                        valueToAssignNode = Expression.Call(
                            propAccessNode,
                            StringSubstringMethod,
                            Expression.Constant(0),
                            Expression.Constant(nonSummaryAttribute.MaxLength.Value));
                    }

                    // captures  propN = x.valueN. 
                    MemberAssignment propAssignmentNode = Expression.Bind(prop, valueToAssignNode);
                    memberBindings.Add(propAssignmentNode);
                }

                if (nonSummaryAttributeCount == 0)
                {
                    //If there are no NonSummaryAttributes on the entity, we cache null, and we won't call Select() next time. 
                    return null;
                }


                /* This is where we are going to capture the initialization of all the entity's properties that aren't marked with NonSummaryAttribute.

                 MemberInitExpression capture the creation and the initialization of the object. It decomposes into a NewExpression that captures the info about the ctor call,
                 along with a series of bindings that each contain what expression is bound to what property.

                 new T() { prop1 = x.value1, prop2 = x.value2, propN = x.valueN } - this is what this expression will look like.
                 This is how we skip any properties that are marked with that attribute. We just don't include them in the initializing form.
                 */
                MemberInitExpression formInitBodyNode =
                    Expression.MemberInit(newEntityExpression, memberBindings);


                // Creates an expression tree that represent the lambda expression at runtime.
                // This root node act as the entry point for our expression tree that's going to be compiled at runtime, resulting in a strongly typed delegate.
                // (Parameters) x => (Body) new T() { prop1 = x.value1, prop2 = x.value2, propN = x.valueN }
                return Expression.Lambda<Func<T, T>>(formInitBodyNode, lambdaParamNode);
            });

        return (Expression<Func<T, T>>)selectExpression;
    }

    /// <summary>
    /// Method first looks for an entity with the same id that is already being tracked and returns it if the method has found it, otherwise the method queries the database.
    /// </summary>
    public static ValueTask<TEntity> FindByIdAsync<TEntity>(this DbSet<TEntity> dbSet, int id,
        CancellationToken cancellationToken = default, bool track = true) where TEntity : BaseEntity
    {
        if (id <= 0)
        {
            return ValueTask.FromResult<TEntity>((TEntity)null);
        }

        var alreadyTracked = dbSet.Local.FindEntry(id)
            ?.Entity;
        return alreadyTracked != null
            ? new ValueTask<TEntity>(alreadyTracked)
            : new ValueTask<TEntity>(dbSet
                .ApplyTracking(track)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken));
    }
    
    public static IQueryable<TEntity> ApplyTracking<TEntity>(this IQueryable<TEntity> query, bool track = true)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(query);
        return track ? query.AsTracking() : query.AsNoTracking();
    }


    public static ValueTask<TEntity> FindByIdAsync<TEntity, TIncluded>(
        this IIncludableQueryable<TEntity, TIncluded> query, int id, CancellationToken cancellationToken = default,
        bool track = true) where TEntity : BaseEntity
    {
        Guard.NotNull(query);
        if (id == 0)
        {
            return ValueTask.FromResult<TEntity>(null);
        }

        var db = query.GetDbContext();
        var trackedEntity = db
            .Set<TEntity>()
            .Local.FindEntry(id)
            ?.Entity;
        return trackedEntity != null
            ? new ValueTask<TEntity>(trackedEntity)
            : new ValueTask<TEntity>(query
                .ApplyTracking(track)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken));
    }

    public static bool TryUpdate<TEntity>(this DbContext db, TEntity entity) where TEntity : BaseEntity
    {
        Guard.NotNull(db);
        Guard.NotNull(entity);
        var entry = db.Entry(entity);
        if (entry.State == EfState.Detached)
        {
            db.TryDetachAlreadyTrackedEntity(entity);
            entry.State = EfState.Modified;
            return true;
        }

        return false;
    }

    private static bool TryDetachAlreadyTrackedEntity<TEntity>(this DbContext db, TEntity entity) where TEntity : BaseEntity
    {
        var otherEntry = db.Set<TEntity>().Local.FindEntry(entity.Id);
        if (otherEntry != null 
            && otherEntry.State != EfState.Added 
            && otherEntry.State != EfState.Modified)
        {
            otherEntry.State = EfState.Detached;
            return true;
        }

        return false;
    }

 
    public static async Task<CollectionEntry<TEntity, TNavigationProperty>>? LoadCollectionAsync<TEntity,
        TNavigationProperty>(this DbHandlerContext db, TEntity entity,
        Expression<Func<TEntity, IEnumerable<TNavigationProperty>>> navigationProperty, bool force = false,
        Func<IQueryable<TNavigationProperty>, IQueryable<TNavigationProperty>> queryModifier = null,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
        where TNavigationProperty : BaseEntity
    {
        Guard.NotNull(entity);
        Guard.NotNull(navigationProperty);
        if (!entity.HasRealId())
        {
            return null;
        }

        var entry = db.Entry(entity);
        if (entry.State == EfState.Deleted)
        {
            return null;
        }

        var collection = entry.Collection(navigationProperty);
        var isLoaded = collection.IsLoaded;

        if (!isLoaded && entry.State == EfState.Detached)
        {
            var trackedEntity = db
                .Set<TEntity>()
                .Local.FindEntry(entity.Id);

            if (trackedEntity != null)
            {
                var otherCollection = await db.LoadCollectionAsync(trackedEntity.Entity,
                    navigationProperty,
                    force,
                    queryModifier,
                    cancellationToken);
                collection.CurrentValue = otherCollection?.CurrentValue;
                collection.IsLoaded = true;
                isLoaded = true;
                force = false;
            }
            else
            {
                db.Attach(entity);
            }
        }

        if (force)
        {
            collection.IsLoaded = false;
        }

        if (!isLoaded || force)
        {
            if (queryModifier != null)
            {
                var query = queryModifier(collection.Query());
                collection.CurrentValue = await query.ToListAsync(cancellationToken);
                collection.IsLoaded = true;
            }
            else
            {
                await collection.LoadAsync(cancellationToken);
            }
        }

        return collection;
    }
}