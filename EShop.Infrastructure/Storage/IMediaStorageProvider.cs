using EShop.Infrastructure.Modules;

namespace EShop.Infrastructure.Storage;

public interface IMediaStorageProvider : IProvider
{
    Task DeleteMediaAsync(string fileName);

    string GetMediaUrl(string fileName);

    Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null);
}