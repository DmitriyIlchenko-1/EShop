using EShop.Infrastructure.IO;
using EShop.Infrastructure.Utilities;
using Microsoft.Extensions.FileProviders;

namespace EShop.Infrastructure.FileSystem;


public interface ILocalFileProvider : IFileProvider
{
    public string Root { get; }
    string MapPath(string path);
}



 