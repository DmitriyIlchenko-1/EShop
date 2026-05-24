namespace EShop.Infrastructure.Media.Images;

public interface IImage
{
    Task SaveAsync(Stream stream);
    void Transform(Action<IImageProcessingContext> tr);
}

public interface IImageProcessingContext
{
    public IImageProcessingContext Resize(int width, int height);
}