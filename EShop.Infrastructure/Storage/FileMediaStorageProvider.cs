 

using EShop.Core.Platform.Web;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.FileSystem;

namespace EShop.Infrastructure.Storage;

public class FileMediaStorageProvider : IMediaStorageProvider
{
    private const string MediaFileLocation = "images";
    private readonly IWebHelper _webHelper;
    public string Host { get;}
    public FileMediaStorageProvider(IWebHelper webHelper)
    {
        _webHelper = webHelper;
        Host = _webHelper.HttpContext.Request.Host.Value;
    }

    public async Task DeleteMediaAsync(string fileName)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetMediaUrlAsync(string fileName)
    {
        var page = _webHelper.GetCurrentPageUrl();
        var path = Path.Combine(page, MediaFileLocation + "/" + fileName);
        return Task.FromResult(path);

    }

    public async Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
    {
        throw new NotImplementedException();
    }

    
}