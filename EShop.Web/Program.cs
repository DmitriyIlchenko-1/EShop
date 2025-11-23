using EShop.Web.Common.Infrustructure;
using EShop.Web.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.ConfigureApplicationServices(builder);

var app = builder.Build();


app.ConfigureApplicationPipeline();
await DataSeeder.SeedAsync(app);


app.Run();