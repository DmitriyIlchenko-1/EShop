using System.Reflection;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Data.DbHandlers;
using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

/// <summary>
/// Abstract base class to inherit for db handler types when the db handler type needs to implement only a subset of the interface methods.
/// Db handler can also inherit this class to receive typed entity references it can then work with. 
/// </summary>
/// <typeparam name="TEntity">The entity type the db handler works with</typeparam>
public abstract class AsyncDbHandler<TEntity> : AsyncDbHandler<TEntity, ApplicationDbContext> where TEntity : class
{
}

public abstract class DbHandler<TEntity> : DbHandler<TEntity, ApplicationDbContext> where TEntity : class
{
}

public partial class ApplicationDbContext : DbHandlerContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblies = Singleton<ITypeScanner>.Instance.Assemblies;
        base.OnModelCreating(modelBuilder);
        RegisterConvention(modelBuilder);


        RegisterCustomMappings(modelBuilder, assemblies);
        Console.WriteLine();
    }

    private static void RegisterConvention(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes();
        foreach (var entityType in entityTypes)
        {
            if (entityType.IsPropertyBag)
            {
                continue;
            }

            if (entityType.ClrType.Namespace != null)
            {
                string[] nameParts = entityType.ClrType.Namespace.Split('.');
                var tableName = string.Concat(nameParts[2], "_", entityType.ClrType.Name);
                modelBuilder
                    .Entity(entityType.Name)
                    .ToTable(tableName);
            }
        }

        foreach (var foreignKey in entityTypes.SelectMany(x => x.GetForeignKeys()))
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
    }

    private static void RegisterCustomMappings(ModelBuilder modelBuilder, IEnumerable<Assembly> allAssemblies)
    {
        foreach (var assembly in allAssemblies) modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
}