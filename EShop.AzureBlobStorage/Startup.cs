using EShop.AzureBlobStorage.Providers;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
 

namespace EShop.AzureBlobStorage;

public class Startup : IEStartup
{
    public int Order => PipelineOrder.Default;

    public void ConfigureApplication(IApplicationBuilder app)
    {
        
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMediaStorageProvider, TestProvider>();
    }
}