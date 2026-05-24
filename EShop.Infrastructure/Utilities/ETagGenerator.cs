using Microsoft.Extensions.FileProviders;

namespace EShop.Infrastructure.Utilities;

public static class ETagGenerator
{
    public static string GenerateETag(IFileInfo file)
    {
        Guard.NotNull(file);
        var last = file.LastModified;
        var hash = HashCodeCombiner
            .Start()
            .Add(file.Length)
            .Add(new DateTimeOffset(last.Year, last.Month, last.Day, last.Hour, last.Minute, last.Second, last.Offset))
            .GetCombinedHash64();

        return '\"' + hash + '\"';

    }
}