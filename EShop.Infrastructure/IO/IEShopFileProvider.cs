using Microsoft.Extensions.FileProviders;

namespace EShop.Infrastructure.FileSystem;

public interface IEShopFileProvider : IFileProvider
{
    public string Root { get; }
    string MapPath(string path);

    IEnumerable<string> EnumerableFiles(string path, string searchPattern = "");
}