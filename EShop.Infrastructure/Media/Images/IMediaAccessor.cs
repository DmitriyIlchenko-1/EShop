using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;


namespace EShop.Infrastructure.Media.Images;

public interface IMediaAccessor
{
    Task<IFileInfo> GetMediaFile(MediaAccessorContext ctx);
}

public class DefaultMediaAccessor : IMediaAccessor
{
    private readonly IFileProvider _fileProvider;
    private readonly IImageProcessor _imageProcessor;
    private readonly IImageCache _imageCache;
    public ILogger Logger { get; set; } = NullLogger.Instance;

    public DefaultMediaAccessor(IApplicationContext app, IImageProcessor imageProcessor, IImageCache imageCache)
    {
        _imageProcessor = imageProcessor;
        _imageCache = imageCache;
        _fileProvider = app.ImageRoot;
    }

    public async Task<IFileInfo> GetMediaFile(MediaAccessorContext ctx)
    {
        Guard.NotNull(ctx);
        var info = ctx.ImageDescriptor;
        var originalFile = _fileProvider.GetFileInfo(info.Path);

        if (!originalFile.Exists)
        {
            return new NotFoundFileInfo(info.Path);
        }

        var query = new ProcessImageQuery(ctx.Parameters, originalFile, ctx.ImageDescriptor);

        if (!query.NeedsProcessing())
        {
            return originalFile;
        }

        var cacheImage = await _imageCache.GetAsync(ctx.ImageDescriptor.Id, query);
        if (cacheImage.Exists && cacheImage.FileInfo.Length > 0)
        {
            return cacheImage.FileInfo;
        }
        else
        {
            //TODO: lock concurrent requests to the same resources
            _imageCache.Refresh(cacheImage);
            if (!cacheImage.Exists)
            {
                try
                {
                    var processedImage = await _imageProcessor.ProcessImageAsync(query);
                    await _imageCache.PutAsync(cacheImage, processedImage);
                }
                catch (Exception e)
                {
                   // Logger.LogError();
                   if (e is ExtractThumbnailException)
                   {
                       await using var stream = new MemoryStream();
                       await _imageCache.PutAsync(cacheImage, stream);
                   }
                }
            }

            return cacheImage.FileInfo;
        }
    }
}

public class MediaAccessorContext
{
    public IDictionary<string, object> Parameters { get; set; }
    public ImageDescriptor ImageDescriptor { get; set; }
}

public class ImageDescriptor
{
    public int Id { get; set; }
    public int MaxWidth { get; set; }
    public int MaxHeight { get; set; }
    public string Path { get; set; }
    public string Extension { get; set; }
}