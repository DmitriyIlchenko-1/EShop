using System.Reflection;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure;
using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext : DbHandlerContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblies = Singleton<ITypeScanner>.Instance.GetAssemblies();
        base.OnModelCreating(modelBuilder);

        RegisterConvention(modelBuilder);

        RegisterCustomMappings(modelBuilder, assemblies);
        Console.WriteLine();
    }

    private static void RegisterConvention(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes();
        foreach (var entityType in entityTypes)
            if (entityType.ClrType.Namespace != null)
            {
                string[] nameParts = entityType.ClrType.Namespace.Split('.');
                var tableName = string.Concat(nameParts[2], "_", entityType.ClrType.Name);
                modelBuilder
                    .Entity(entityType.Name)
                    .ToTable(tableName);
            }

        foreach (var foreignKey in entityTypes.SelectMany(x => x.GetForeignKeys()))
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
    }

    private static void RegisterCustomMappings(ModelBuilder modelBuilder, IEnumerable<Assembly> allAssemblies)
    {
        foreach (var assembly in allAssemblies) modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
}