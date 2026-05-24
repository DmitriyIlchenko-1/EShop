using EShop.Infrastructure.Utilities;
using Microsoft.Extensions.FileProviders;

namespace EShop.Infrastructure.Media.Images;

public interface IImageFactory
{
    Task<IImage> LoadAsync(IFileInfo info);
}

public class DefaultImageFactory : IImageFactory
{
    public async Task<IImage> LoadAsync(IFileInfo info)
    {
        Guard.NotNull(info);
        if (info.Length == 0 || !info.Exists)
            return null;
        
        await using var stream = info.CreateReadStream();
        SixLabors.ImageSharp.Image sharpImage = await SixLabors.ImageSharp.Image.LoadAsync(stream);
        Image image = new Image(info, sharpImage);
        return image;
    }
}