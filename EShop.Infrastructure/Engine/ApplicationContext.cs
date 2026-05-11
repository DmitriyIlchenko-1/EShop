using EShop.Infrastructure.FileSystem;
using EShop.Infrastructure.IO;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Hosting;

namespace EShop.Infrastructure.Engine;

public class DefaultApplicationContext : IApplicationContext
{
    public ILocalFileProvider AppDataRoot { get; private set; }
    public ILocalFileProvider WebRoot { get; private set; }
   
    public IWebHostEnvironment Environment { get; init; }

    public DefaultApplicationContext(IWebHostEnvironment env)
    {
        Guard.NotNull(env);
        Environment = env;
        EnsureFileProvidersCreated();
    }

    private void EnsureFileProvidersCreated()
    {
        WebRoot = new DefaultLocalFileProvider(Environment.ContentRootPath);
        if (Directory.Exists(Path.Combine(WebRoot.Root, "App_Data")))
        {
            AppDataRoot = new DefaultLocalFileProvider(Path.Combine(WebRoot.Root, "App_Data"));
        }
    }
}