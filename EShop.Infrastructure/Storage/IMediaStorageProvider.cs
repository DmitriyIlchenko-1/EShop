using EShop.Infrastructure.Modules;

namespace EShop.Infrastructure.Storage;

public interface IMediaStorageProvider : IProvider
{
    Task DeleteMediaAsync(string fileName);

    // Task<(string Url, string Subpath)> GetMediaUrlAsync(string fileName, string fileId = null);

    Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null);
    public string Root { get; }
 
}

 