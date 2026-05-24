 
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace EShop.Infrastructure.Engine;

public class DefaultApplicationContext : IApplicationContext
{
    public IFileProvider AppDataRoot { get; private set; }
    public IFileProvider WebRoot { get; private set; }
    public IFileProvider ImageRoot { get; private set; }
   
    public IWebHostEnvironment Environment { get; init; }

    public DefaultApplicationContext(IWebHostEnvironment env)
    {
        Guard.NotNull(env);
        Environment = env;
        EnsureFileProvidersCreated();
    }

    private void EnsureFileProvidersCreated()
    {
        var webRootPath = Environment.ContentRootPath;
        WebRoot = new PhysicalFileProvider(webRootPath);
        if (Directory.Exists(Path.Combine(webRootPath, "App_Data")))
        {
            AppDataRoot = new PhysicalFileProvider(Path.Combine(webRootPath, "App_Data"));
        }
        
        ImageRoot = new PhysicalFileProvider(Path.Combine(webRootPath, "wwwroot/images"));
    }
}