using EShop.Infrastructure.IO;
using Microsoft.Extensions.FileProviders;

namespace EShop.Infrastructure.FileSystem;


public interface IEShopFileProvider : IFileProvider
{
    public string Root { get; }
    string MapPath(string path);

     
}