using EShop.Core.Content.Media.Domain;
using EShop.Infrastructure.Storage;

namespace EShop.Core.Content.Media.Services;

/// <summary>
/// Use this service's methods to query, save and delete media files.
/// The service uses <see cref="IMediaStorageProvider"/> to get the binary data represented by MediaFile.
/// </summary>
public interface IMediaService
{
    Task<IList<MediaFile>> GetMediaFilesByIdsAsync(int[] ids, bool track);
    Task<List<MediaFile>> GetFilesByProductIdAsync(int productId, int count, bool track);

    Task<string> GetMediaUrlAsync(MediaFile mediaFile);

    Task<string> GetMediaUrlAsync(string fileName);

    Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType);

    Task DeleteMediaFromProviderAsync(string fileName);

    Task DeleteMediaFullAsync(MediaFile mediaFile);
}