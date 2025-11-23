 

namespace EShop.Infrastructure.Storage;

public class FileMediaStorageProvider : IMediaStorageProvider
{
    public async Task DeleteMediaAsync(string fileName)
    {
        throw new NotImplementedException();
    }

    public string GetMediaUrl(string fileName)
    {
        throw new NotImplementedException();
    }

    public async Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
    {
        throw new NotImplementedException();
    }
}