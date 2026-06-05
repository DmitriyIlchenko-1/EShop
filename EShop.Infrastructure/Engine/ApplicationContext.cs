 
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace EShop.Infrastructure.Engine;

public class DefaultApplicationContext : IApplicationContext
{
    public IFileProvider AppDataRoot { get; private set; }
    public IFileProvider WebRoot { get; private set; }
    public IFileProvider ContentRoot => Environment.ContentRootFileProvider;
   
    public IWebHostEnvironment Environment { get; init; }

    public DefaultApplicationContext(IWebHostEnvironment env)
    {
        Guard.NotNull(env);
        Environment = env;
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