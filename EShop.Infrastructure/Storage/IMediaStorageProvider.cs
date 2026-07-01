using EShop.Infrastructure.Modules;

namespace EShop.Infrastructure.Storage;

public interface IMediaStorageProvider : IProvider
{
    Task DeleteMediaAsync(string fileName);

    Task<string> GetMediaUrlAsync(string fileName, int fileId);

    Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null);
    public string Root { get; }
 
}

 