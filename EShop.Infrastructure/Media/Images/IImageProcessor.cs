using System.Collections.Specialized;
using System.Web;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Extensions;
 
using EShop.Infrastructure.Utilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace EShop.Infrastructure.Media.Images;

public interface IImageProcessor
{
    Task<IImage> ProcessImageAsync(ProcessImageQuery query);
}

public class DefaultImageProcessor : IImageProcessor
{
    private readonly IImageFactory _imageFactory;

    public DefaultImageProcessor(IImageFactory imageFactory)
    {
        _imageFactory = imageFactory;
    }

    private readonly static Action<ProcessImageQuery>[] _imageProcessors =
    [
        ResizeImage
    ];


    public async Task<IImage> ProcessImageAsync(ProcessImageQuery query)
    {
        Guard.NotNull(query);
        var image = await _imageFactory.LoadAsync(query.OriginalImage);
        query.Result = image;

        foreach (var processor in _imageProcessors)
        {
            processor(query);
        }

        return query.Result;
    }


    private static void ResizeImage(ProcessImageQuery query)
    {
        if (query.Width != 0 || query.Height != 0)
        {
            query.Result.Transform(x => { x.Resize(query.Width, query.Height); });
        }
    }
}

 

 
 