using Autofac;
using Autofac.Extensions.DependencyInjection;
using EasyCaching.Core;
using EasyCaching.InMemory;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Common.Services;
using EShop.Core.Data;
using EShop.Core.Data.DbHandlers;
using EShop.Core.Platform.Caching;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data.DbHandlers;
using EShop.Infrastructure.Engine;
using EShop.Web.Common.Infrastructure;
using EShop.Web.Common.Infrustructure;
using EShop.Web.Common.Models;
using EShop.Web.Common.Models.Choices;
using EShop.Web.Infrastructure.DbHandlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
var startup = EngineContext
    .Create()
    .Startup(builder.Environment);
//Add services to Microsoft's IServiceCollection. The services will still end up in the same container, which is likely to be Autofac's container, though it depends on the settings.
startup.ConfigureServices(builder.Services, builder.Configuration);


//Add services directly through Autofac.
builder.Host.ConfigureContainer<ContainerBuilder>(startup.ConfigureContainer);
var app = builder.Build();


EngineContext.Current.ChildLifetimeScopeAccessor
    = app.Services.GetRequiredService<IChildLifetimeScopeAccessor>();

app.Lifetime.ApplicationStarted.Register(() =>
{
    startup.Dispose();
    startup = null;
});

startup.ConfigureApplicationPipeline(app);

app.MapGet("/",
    async (HttpContext context, ApplicationDbContext db) =>
    {
        var newProduct = new Product();
        newProduct.Name = "AddedProduct's name";
        db.Products.Add(newProduct);
        await db.SaveChangesAsync();
        context.Response.Redirect("setup");
    });

app.MapGet("/setup",
    async (HttpContext context, ApplicationDbContext db) =>
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Name == "TestCategoryName");
        if (category == null)
        {
            category = new Category()
            {
                Name = "TestCategoryName",
            };
            await db.AddAsync(category);
        }
        else
        {
            category.Name = "TestCategoryName";
        }

        var product = await db.Products.FirstOrDefaultAsync(x => x.Name == "TestProductName");
        if (product == null)
        {
            product = new Product()
            {
                Name = "TestProductName",
                Description = "TestProductDescription",
            };
            await db.AddAsync(product);
        }
        else
        {
            product.Name = "TestProductName";
            product.Deleted = false;
        }

        product.ProductCategories.Add(new ProductCategory() { Category = category });


        await db.SaveChangesAsync();
    });
 
app.Run();



public class Handler : DbHandler<Product>
{
    protected override DbHandlerResult OnInserted(Product entity, IHandleEntityContext entityContext)
    {
        return DbHandlerResult.Ok;
    }

    protected override DbHandlerResult OnInserting(Product entity, IHandleEntityContext entityContext)
    {
        return DbHandlerResult.Ok;
    }
}