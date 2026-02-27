using EShop.Infrastructure.Modules;

namespace EShop.Infrastructure.Storage;

public interface IMediaStorageProvider : IProvider
{
    Task DeleteMediaAsync(string fileName);

    Task<string> GetMediaUrlAsync(string fileName);

    Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null);
}

public class MockMediaStorageProvider : IMediaStorageProvider
{
    public Task DeleteMediaAsync(string fileName)
    {
        return Task.CompletedTask;
    }

    public Task<string> GetMediaUrlAsync(string fileName)
    {
        return Task.FromResult(fileName);
    }

    public Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
    {
        return Task.CompletedTask;
    }
}