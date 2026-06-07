using Autofac;
using Autofac.Extensions.DependencyInjection;
using EasyCaching.Core;
using EasyCaching.InMemory;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Extensions;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Common.Services;
using EShop.Core.Data;
using EShop.Core.Data.DbHandlers;
using EShop.Core.Platform.Caching;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data.DbHandlers;
using EShop.Infrastructure.Engine;
using EShop.Web.Common.Infrastructure;
using EShop.Web.Common.Infrustructure;
using EShop.Web.Common.Middleware;
using EShop.Web.Common.Models;
using EShop.Web.Common.Models.Choices;
using EShop.Web.Infrastructure.DbHandlers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
var appContext = new DefaultApplicationContext(builder.Environment, builder.Configuration);
var engine = EngineContext.Create();
var startup = engine.Startup(appContext);
//Add services to Microsoft's IServiceCollection. The services will still end up in the same container, which is likely to be Autofac's container, though it depends on the settings.
startup.ConfigureServices(builder.Services, builder.Configuration);
 

//Add services directly through Autofac.
builder.Host.ConfigureContainer<ContainerBuilder>(startup.ConfigureContainer);
var app = builder.Build();
 
engine.ChildLifetimeScopeAccessor
    = app.Services.GetRequiredService<IChildLifetimeScopeAccessor>();

app.Lifetime.ApplicationStarted.Register(() =>
{
    startup.Dispose();
    startup = null;
});
 
startup.ConfigureApplicationPipeline(app);

using var d = engine.ChildLifetimeScopeAccessor.CreateManualChildLifetimeScope(out var scope);
var dbContext = scope.Resolve<ApplicationDbContext>();
var userManager = scope.Resolve<UserManager<User>>();
var roleManager = scope.Resolve<RoleManager<Role>>();
var dataSeeder = new DataSeeder(dbContext, userManager, roleManager);
await dataSeeder.SeedDataAsync();
 
app.Run();