using EShop.Core.Platform.Web;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;


namespace EShop.Infrastructure.Storage;

public class FileMediaStorageProvider : IMediaStorageProvider
{
    private readonly HttpContext _context;
     
    public string Root { get; }

    public string ExtractSubpath(string path)
    {
        throw new NotImplementedException();
    }

    public FileMediaStorageProvider(IHttpContextAccessor contextAccessor)
    {
        _context = contextAccessor?.HttpContext;
        var builder = new UriBuilder(_context?.Request.Scheme, _context?.Request.Host.Host, _context.Request.Host.Port.Value);
        Root = builder.Uri.AbsoluteUri;
    }

    public async Task DeleteMediaAsync(string fileName)
    {
        throw new NotImplementedException();
    }

   
//http://localhost:5158/images/15/pexels-azka-nandya-91944639-9507137.jpg

    public Task<string> GetMediaUrlAsync(string fileName, int fileId)
    {
        if (fileId <= 0)
        {
           throw new ArgumentException("FileId must be greater than 0.");
        } 

        string path = Root;
        path +=  "images" + "/" + fileId + "/" + fileName;
        return Task.FromResult(path);
    }

    public async Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
    {
        throw new NotImplementedException();
    }
}