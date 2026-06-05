using System.Globalization;
using EShop.Core.Content.Media.Domain;
using EShop.Core.Content.Media.Services;
using EShop.Core.Data;
using EShop.Core.Platform.Web;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;


public class MediaService : IMediaService
{
    private const string MediaFileLocation = "images";
    private readonly ApplicationDbContext _db;
    private readonly IMediaStorageProvider _mediaStorageService;
    private readonly IWebHelper _webHelper;


    public MediaService(ApplicationDbContext db, IMediaStorageProvider mediaStorageService, IWebHelper webHelper)
    {
        _db = db;
        _mediaStorageService = mediaStorageService;
        _webHelper = webHelper;
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

        return await _db
            .MediaFiles.ApplyTracking(track)
            .FirstOrDefaultAsync(x => x.Id == id);
    }


    public async Task<(string Url, string Subpath)> GetMediaUrlAsync(MediaFile mediaFile) =>
        mediaFile != null ? await GetMediaUrlAsync(mediaFile.FileName, mediaFile.Id.ToString(CultureInfo.InvariantCulture)) : await GetMediaUrlAsync("not-found-image.png");


    public Task<(string Url, string Subpath)> GetMediaUrlAsync(string fileName, string fileId = null)
    {
        var page = _webHelper.GetCurrentPageUrl();
        string subpath =  MediaFileLocation + "/";
        if (!fileId.IsEmpty())
        {
            subpath += fileId + "/";
        }
        subpath += fileName;
        
        var path = Path.Combine(page, subpath);
        return Task.FromResult((path, subpath));
    }

    public Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
    {
        return _mediaStorageService.SaveMediaAsync(mediaBinaryStream, fileName, mimeType);
    }
}