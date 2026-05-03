using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.FileSystem;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting.Internal;

namespace EShop.Infrastructure.IO;

/// <summary>
/// The abstraction over EShop's file system. 
/// </summary>
public class DefaultLocalFileProvider : PhysicalFileProvider, ILocalFileProvider
{
    public DefaultLocalFileProvider(string rootPath) : base(rootPath)
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