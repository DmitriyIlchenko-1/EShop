using EShop.Core.Platform.Caching;
using EShop.Infrastructure.Caching;
using EShop.Web.Common.Infrastructure;
using EShop.Web.Common.Infrustructure;
using Microsoft.AspNetCore.Mvc;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);


builder.Services.ConfigureApplicationServices(builder);

var app = builder.Build();

 
app.ConfigureApplicationPipeline();

 

app.Run();