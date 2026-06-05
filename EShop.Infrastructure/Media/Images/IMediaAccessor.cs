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
   private readonly ILogger _logger = NullLogger.Instance;

    public DefaultMediaAccessor(IApplicationContext app, IImageProcessor imageProcessor, IImageCache imageCache)
    {
        _imageProcessor = imageProcessor;
        _imageCache = imageCache;
        
        _fileProvider = app.WebRoot;
    }

    public async Task<IFileInfo> GetMediaFile(MediaAccessorContext ctx)
    {
        Guard.NotNull(ctx);
        var info = ctx.ImageDescriptor;
        var originalFile = _fileProvider.GetFileInfo("images/" + info.Path);

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
            _logger.ServedFromCache(cacheImage.FileInfo.Name);
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
                    _logger.LogError(e.Message);
                    
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

 