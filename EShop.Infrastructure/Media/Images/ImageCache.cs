using System.Globalization;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.FileSystem;
using EShop.Infrastructure.Utilities;
using Microsoft.Extensions.FileProviders;

namespace EShop.Infrastructure.Media.Images;

public interface IImageCache
{
    Task PutAsync(CachedImage cacheImage, IImage image);
    Task PutAsync(CachedImage cacheImage, Stream stream);
    Task<CachedImage> GetAsync(int imgId, ProcessImageQuery query);
    void Refresh(CachedImage cacheImage);
}

public class ImageCache : IImageCache
{
    private readonly IFileProvider _imageFileProvider;
    private const string SubDirectory = "Cached";
    private const string IdFormat = "0000000";

    public ImageCache(IApplicationContext app)
    {
        _imageFileProvider = app.ImageRoot;
    }

    public async Task PutAsync(CachedImage cacheImage, IImage image)
    {
        Guard.NotNull(cacheImage);
        Guard.NotNull(image);
        var fileInfo = _imageFileProvider.GetFileInfo(SubDirectory + "/" + cacheImage.NameInCache);
        if (PreparePut(fileInfo))
        {
            await using var stream = File.Create(fileInfo.PhysicalPath);
            //TODO: look into whether random query parameters in the resuest can result in the same image being processed and/or cached twice? 
            await image.SaveAsync(stream);
             
        }

        Refresh(cacheImage);
    }

    public async Task PutAsync(CachedImage cacheImage, Stream stream)
    {
        Guard.NotNull(cacheImage);
        Guard.NotNull(stream);
        var fileInfo = _imageFileProvider.GetFileInfo(SubDirectory + "/" + cacheImage.NameInCache);
        if (PreparePut(fileInfo))
        {
            await using var fileStream = File.Create(fileInfo.PhysicalPath);
            await stream.CopyToAsync(fileStream);
        }

        Refresh(cacheImage);
    }


    public async Task<CachedImage> GetAsync(int imgId, ProcessImageQuery query)
    {
        Guard.NotNull(query);
        if (imgId == 0 || !query.NeedsProcessing())
            return new CachedImage()
            {
                Exists = false,
                FileInfo = new NotFoundFileInfo(query.OriginalImage.Name) // does it include extension? do I need this?
            };

        var nameInCache = GenerateCachedPath(imgId, query);
        IFileInfo fileInfo = new NotFoundFileInfo(nameInCache);
        if (!nameInCache.IsEmpty())
        {
            fileInfo = _imageFileProvider.GetFileInfo(SubDirectory + "/" + nameInCache);
        }

        return new CachedImage
        {
            Exists = fileInfo.Exists,
            NameInCache = nameInCache,
            FileInfo = fileInfo
        };
    }

    public void Refresh(CachedImage cacheImage)
    {
        Guard.NotNull(cacheImage);
        cacheImage.FileInfo = _imageFileProvider.GetFileInfo(SubDirectory + "/" + cacheImage.NameInCache);
        cacheImage.Exists = cacheImage.FileInfo.Exists;
    }

    private string GenerateCachedPath(int imgId, ProcessImageQuery query)
    {
        if (!query.NeedsProcessing())
            return string.Empty;

        string imageId = imgId.ToString(IdFormat, CultureInfo.InvariantCulture);
        var imgData = query.CreateHash();
        return
            $"{imageId}-{Path.GetFileNameWithoutExtension(query.OriginalImage.Name)}-{imgData}{query.ImageInfo.Extension}";
    }


    private bool PreparePut(IFileInfo fileInfo)
    {
        Guard.NotNull(fileInfo);
        if (fileInfo.Exists)
        {
            return false;
        }

        var dirName = Path.GetDirectoryName(fileInfo.PhysicalPath);

        if (dirName.HasValue())
        {
            Directory.CreateDirectory(dirName);
        }

        return true;
    }
}

public class CachedImage
{
    public bool Exists { get; internal set; }
    public IFileInfo FileInfo { get; internal set; }
    public string NameInCache { get; internal set; }
}