using EShop.Core.Platform.Web;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Extensions;


namespace EShop.Infrastructure.Storage;

public class FileMediaStorageProvider : IMediaStorageProvider
{
    private readonly IWebHelper _webHelper;
    public string Root { get; }

    public string ExtractSubpath(string path)
    {
        throw new NotImplementedException();
    }

    public FileMediaStorageProvider(IWebHelper webHelper)
    {
        _webHelper = webHelper;
        Root = _webHelper.HttpContext.Request.Host.Value;
    }

    public async Task DeleteMediaAsync(string fileName)
    {
        throw new NotImplementedException();
    }

   

    // public Task<(string Url, string Subpath)> GetMediaUrlAsync(string fileName, IDictionary<string, object> parameters)
    // {
    //     var page = _webHelper.GetCurrentPageUrl();
    //     string subpath = MediaFileLocation + "/";
    //     if (parameters.TryGetValueAs("id", out int id))
    //     {
    //         subpath += id + "/";
    //     }
    //     
    //     var path = Path.Combine(page, subpath);
    //     return Task.FromResult((path, subpath));
    // }

    public async Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
    {
        throw new NotImplementedException();
    }
}