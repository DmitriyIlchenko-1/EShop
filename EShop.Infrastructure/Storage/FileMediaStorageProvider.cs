 

using EShop.Infrastructure.Engine;
using EShop.Infrastructure.FileSystem;

namespace EShop.Infrastructure.Storage;

public class FileMediaStorageProvider : IMediaStorageProvider
{
    private readonly ILocalFileProvider _fileProvider;
    private const string MediaFileLocation = "Media";
    public FileMediaStorageProvider(IApplicationContext app)
    {
        _fileProvider = app.WebRoot;
    }

    public async Task DeleteMediaAsync(string fileName)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetMediaUrlAsync(string fileName)
    {
        return Task.FromResult(_fileProvider.MapPath(MediaFileLocation + "/" + fileName));

    }

    public async Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
    {
        throw new NotImplementedException();
    }
}