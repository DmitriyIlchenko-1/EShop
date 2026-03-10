using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.FileSystem;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace EShop.Infrastructure.IO;

public class DefaultFileProvider : PhysicalFileProvider, IEShopFileProvider
{
    public DefaultFileProvider(IWebHostEnvironment hostEnv) : base(File.Exists(hostEnv.ContentRootPath)
        ? Path.GetDirectoryName(hostEnv.ContentRootPath)!
        : hostEnv.ContentRootPath)
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

    public IEnumerable<string> EnumerableFiles(string path, string searchPattern = "")
    {
        return Directory.EnumerateFiles(path, searchPattern);
    }

    

    // public virtual IDirectory? GetDirectory(string subPath)
    // {
    //     if (subPath.IsEmpty())
    //     {
    //         return null;
    //     }
    //     var dir = new DirectoryInfo(subPath);
    //     if (FileInfoHelper.IsExcluded(dir))
    //     {
    //         return null;
    //     }
    //     
    // }
    //
    // internal string PrepareSubPath(ref string subPath)
    // {
    //     if (subPath.IsEmpty())
    //     {
    //         subPath ??= string.Empty;
    //         return Root;
    //     }
    //
    //     var preparedPath = Path.Join(Root, subPath);
    //
    // }

     
}