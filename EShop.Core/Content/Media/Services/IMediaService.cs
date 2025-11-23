using EShop.Core.Content.Media.Domain;


public interface IMediaService
{
    Task DeleteMediaFromProviderAsync(string fileName);

    Task DeleteMediaFullAsync(Media media);

    string GetMediaUrl(Media media);

    string GetMediaUrl(string fileName);

    Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null);
}