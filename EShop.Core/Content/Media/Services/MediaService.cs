using EShop.Core.Content.Media.Domain;
using EShop.Core.Data;
using EShop.Infrastructure.Storage;


public class MediaService : IMediaService
{
    private readonly ApplicationDbContext _db;

    private readonly IMediaStorageProvider _mediaStorageService;


    public MediaService(ApplicationDbContext db, IMediaStorageProvider mediaStorageService)
    {
        _db = db;
        _mediaStorageService = mediaStorageService;
    }

    private Task DeleteMediaAsync(string fileName)
    {
        return _mediaStorageService.DeleteMediaAsync(fileName);
    }

    public Task DeleteMediaFromProviderAsync(string fileName)
    {
        return DeleteMediaAsync(fileName);
    }

    public Task DeleteMediaFullAsync(Media media)
    {
        _db.Medias.Remove(media);
        return DeleteMediaAsync(media.Filename);
    }

    public string GetMediaUrl(Media media) =>
        media != null ? GetMediaUrl(media.Filename) : GetMediaUrl("not-found-image.png");


    public string GetMediaUrl(string fileName)
    {
        return _mediaStorageService.GetMediaUrl(fileName);
    }

    public Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
    {
        return _mediaStorageService.SaveMediaAsync(mediaBinaryStream, fileName, mimeType);
    }
}