using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.FileSystem;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;

namespace EShop.Infrastructure.IO;

/// <summary>
/// When instantiating this provider directly, an absolute directory path is required and serves as the base path for all requests made using the provider. 
/// </summary>
public class DefaultFileProvider : PhysicalFileProvider, IEShopFileProvider
{
    public DefaultFileProvider(IWebHostEnvironment hostEnv) : base(File.Exists(hostEnv.ContentRootPath)
        ? Path.GetDirectoryName(hostEnv.ContentRootPath)!
        : hostEnv.ContentRootPath)
    {
    }

    public DefaultFileProvider(string path) : base(path)
    {
        
    }

    public new string Root => base.Root;

    public virtual string MapPath(string path)
    {
        path = path
            .Replace("~/", string.Empty)
            .TrimStart('/');
        var pathEnd = path.EndsWith('/') ? Path.DirectorySeparatorChar.ToString() : string.Empty;

        return Path.Combine(Root, path) + pathEnd;
    }
}