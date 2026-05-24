using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.Extensions.FileProviders;

namespace EShop.Infrastructure.Media.Images;

public class ProcessImageQuery
{
    public IFileInfo OriginalImage { get; }
    public IImage Result { get; set;}
    public ImageDescriptor ImageInfo { get; }

    public ProcessImageQuery(IDictionary<string, object> parameters, IFileInfo originalImage, ImageDescriptor imageInfo)
    {
        Parameters = parameters;
        OriginalImage = originalImage;
        ImageInfo = imageInfo;
        Width = int.TryParse(Parameters.FirstOrDefault(x => x.Key.Equals("w", StringComparison.OrdinalIgnoreCase))
                .Value as string,
            out var width)
            ? width
            : 0;
        Height = int.TryParse(Parameters.FirstOrDefault(x => x.Key.Equals("h", StringComparison.OrdinalIgnoreCase))
                .Value as string,
            out var height)
            ? height
            : 0;
        AspectRatio = Parameters.FirstOrDefault(x => x.Key.Equals("q", StringComparison.OrdinalIgnoreCase))
            .Value as string;
    }

    public int Width { get; }
    public int Height { get; }
    public string AspectRatio { get; }
    public IDictionary<string, object> Parameters { get; }

    private bool? _needsProcessing;
    private string? _hash;

    public string CreateHash()
    {
        if (_hash.HasValue())
            return _hash;
        
        using var d = StringBuilderPool.Pool.Get(out var builder);

        foreach (var p in Parameters)
        {
            builder.Append($"{p.Key}{p.Value}-");
        }

        return builder
            .ToString()
            .TrimEnd('-');
    }

    public bool NeedsProcessing()
    {
        if (_needsProcessing.HasValue)
            return _needsProcessing.Value;
        if (Parameters.Count == 0)
            _needsProcessing = false;
        else if (Height != 0 || Width != 0 || !AspectRatio.IsEmpty())
            _needsProcessing = true;

        var maxHeight = ImageInfo.MaxHeight;
        var maxWidth = ImageInfo.MaxWidth;
        if (maxHeight != 0 && Height > maxHeight || maxWidth != 0 && Width > maxWidth)
        {
            _needsProcessing = false;
        }

        return _needsProcessing!.Value;
    }
}

 