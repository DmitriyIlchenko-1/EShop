 
using EShop.Infrastructure.Utilities;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace EShop.Infrastructure.Media.Images;


internal class Image : Disposable, IImage
{
    private readonly SixLabors.ImageSharp.Image _image;
    private readonly FileInfo _physicalFile;
    private IImageProcessingContext _processingContext;

    public Image(IFileInfo originalImage, SixLabors.ImageSharp.Image modifiedImage)
    {
        _image = modifiedImage;
        _physicalFile = new FileInfo(originalImage.PhysicalPath);
    }

    public void Transform(Action<IImageProcessingContext> tr)
    {
        _processingContext ??= new ImageProcessingContext(_image);
        tr(_processingContext);
    }

    public async Task SaveAsync(Stream stream)
    {
        await _image.SaveAsync(stream, _image.Metadata.DecodedImageFormat!);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _image.Dispose();
        }
    }
}

public class ImageProcessingContext : IImageProcessingContext
{
    private readonly SixLabors.ImageSharp.Image _image;

    public ImageProcessingContext(SixLabors.ImageSharp.Image image)
    {
        _image = image;
    }

    public IImageProcessingContext Resize(int width, int height)
    {
        _image.Mutate(x => x.Resize(width, height));
        return this;
    }
}
