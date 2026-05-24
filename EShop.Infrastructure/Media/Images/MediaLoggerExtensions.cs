using Microsoft.Extensions.Logging;

namespace EShop.Infrastructure.Media.Images;

internal static partial class MediaLoggerExtensions
{
    [LoggerMessage(1,
        LogLevel.Debug,
        "The file {Substring} has been found in the cache and is being served",
        EventName = "NotProcessed")]
    public static partial void ServedFromCache(this ILogger logger, string substring);

   
}