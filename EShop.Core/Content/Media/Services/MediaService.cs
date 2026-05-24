using EShop.Core.Content.Media.Domain;
using EShop.Core.Content.Media.Services;
using EShop.Core.Data;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;


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

    public Task DeleteMediaFullAsync(MediaFile mediaFile)
    {
        _db.MediaFiles.Remove(mediaFile);
        return DeleteMediaAsync(mediaFile.FileName);
    }

    public async Task<ICollection<MediaFile>> GetMediaFilesByIdsAsync(int[] ids, bool track = false)
    {
        ArgumentNullException.ThrowIfNull(ids);
        if (ids.Length == 0)
        {
            return [];
        }
        
        var result = await _db
            .MediaFiles.ApplyTracking(track)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        return (from id in ids
                join entity in result on id equals entity.Id
                select entity).ToList();
    }

    public async Task<MediaFile> GetMediaFilesByIdAsync(int id, bool track = false)
    {
        if (id == 0)
            return null;
        
        return await _db.MediaFiles.ApplyTracking(track)
            .FirstOrDefaultAsync(x => x.Id == id);
    }


    public async Task<string> GetMediaUrlAsync(MediaFile mediaFile) =>
        mediaFile != null ? await GetMediaUrlAsync(mediaFile.FileName) : await GetMediaUrlAsync("not-found-image.png");


    public async Task<string> GetMediaUrlAsync(string fileName)
    {
        return await _mediaStorageService.GetMediaUrlAsync(fileName);
    }

    public Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
    {
        return _mediaStorageService.SaveMediaAsync(mediaBinaryStream, fileName, mimeType);
    }
}