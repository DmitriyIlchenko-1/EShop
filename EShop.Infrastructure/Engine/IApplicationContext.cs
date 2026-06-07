 
using EShop.Infrastructure.Engine.Configuration;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace EShop.Infrastructure.Engine;

public interface IApplicationContext
{
    public IFileProvider AppDataRoot { get; }
    public IFileProvider WebRoot { get; }
    public IFileProvider ContentRoot { get; }
    public IWebHostEnvironment Environment { get; }
    public EShopConfiguration Configuration { get; }
}

 