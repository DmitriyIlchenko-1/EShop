using Autofac;
using Autofac.Extensions.DependencyInjection;
using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Engine;
using Microsoft.AspNetCore.Identity;

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

using var _ = engine.ChildLifetimeScopeAccessor.CreateManualChildLifetimeScope(out var scope);
var dbContext = scope.Resolve<ApplicationDbContext>();
var userManager = scope.Resolve<UserManager<User>>();
var roleManager = scope.Resolve<RoleManager<Role>>();
var dataSeeder = new DataSeeder(dbContext, userManager, roleManager);
await dataSeeder.SeedDataAsync();

app.Run();