 
using EShop.Infrastructure.Engine.Configuration;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace EShop.Infrastructure.Engine;

public class DefaultApplicationContext : IApplicationContext
{
    public IFileProvider AppDataRoot { get; private set; }
    public IFileProvider WebRoot { get; private set; }
    public IFileProvider ContentRoot => Environment.ContentRootFileProvider;
   
    public IWebHostEnvironment Environment { get; init; }
    public EShopConfiguration Configuration { get;private set; }

    public DefaultApplicationContext(IWebHostEnvironment env, IConfiguration configuration)
    {
        Guard.NotNull(env);
        Environment = env;

        var config = new EShopConfiguration();
        configuration.Bind("EShop", config);
        Configuration = config;
        EnsureFileProvidersCreated();
    }

    private void EnsureFileProvidersCreated()
    {
        var contentRootPath = Environment.ContentRootPath;
        WebRoot = new PhysicalFileProvider(Path.Combine(contentRootPath, "wwwroot"));
        if (Directory.Exists(Path.Combine(contentRootPath, "App_Data")))
        {
            AppDataRoot = new PhysicalFileProvider(Path.Combine(contentRootPath, "App_Data"));
        }
    }
}