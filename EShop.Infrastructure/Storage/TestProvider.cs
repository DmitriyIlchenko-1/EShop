namespace EShop.Infrastructure.Storage;

public class TestProvider : IMediaStorageProvider
{
    public Task DeleteMediaAsync(string fileName)
    {
        return Task.CompletedTask;
    }

    public string GetMediaUrl(string fileName)
    {
        return string.Empty;
    }

    public Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
    {
        return Task.CompletedTask;
    }
}