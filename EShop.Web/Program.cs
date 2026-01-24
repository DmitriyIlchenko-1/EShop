using Autofac;
using Autofac.Extensions.DependencyInjection;
using EShop.Core.Platform.Caching;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Engine;
using EShop.Web.Common.Infrastructure;
using EShop.Web.Common.Infrustructure;
using Microsoft.AspNetCore.Mvc;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);
//TODO: make it up to the user. 
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
var startup = EngineContext
    .Create()
    .Startup(builder.Environment);
//Add services to Microsoft's IServiceCollection. The services will still end up in the same container, which is likely to be Autofac's container, though it depends on the settings.
startup.ConfigureServices(builder.Services, builder.Configuration);

//Add services directly through Autofac.
builder.Host.ConfigureContainer<ContainerBuilder>(startup.ConfigureContainer);
var app = builder.Build();
EngineContext.Current.ScopeAccessor = new DefaultScopedProviderAccessor(
    app.Services.GetRequiredService<IHttpContextAccessor>(),
    app.Services.GetRequiredService<IServiceScopeFactory>());

startup.ConfigureApplicationPipeline(app);


app.Run();