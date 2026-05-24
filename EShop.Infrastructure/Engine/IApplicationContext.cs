using EShop.Infrastructure.FileSystem;
using EShop.Infrastructure.IO;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace EShop.Infrastructure.Engine;

public interface IApplicationContext
{
    public ILocalFileProvider AppDataRoot { get; }
    public ILocalFileProvider WebRoot { get; }
    public IFileProvider ImageRoot { get; }
    public IWebHostEnvironment Environment { get; }
}

 