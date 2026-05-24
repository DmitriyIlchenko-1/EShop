namespace EShop.Infrastructure.Extensions;

public sealed class ExtractThumbnailException : Exception
{
    public ExtractThumbnailException(string message) : base(message)
    {
    }

    public ExtractThumbnailException(string message, Exception innerException) : base(message, innerException)
    {
    }
}