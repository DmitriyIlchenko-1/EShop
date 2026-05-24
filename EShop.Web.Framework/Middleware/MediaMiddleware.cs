using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using EShop.Core.Content.Media.Configuration;
using EShop.Core.Content.Media.Domain;
using EShop.Core.Content.Media.Services;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Media.Images;
using EShop.Infrastructure.Storage;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace EShop.Web.Common.Middleware;

public class MediaMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TemplateMatcher _matcher;
    private readonly IContentTypeProvider _contentTypeProvider;
    private const string NotFound = "File with the path {0} isn't not found";

    public MediaMiddleware(RequestDelegate next)
    {
        _next = next;
        _contentTypeProvider = new FileExtensionContentTypeProvider();
        var template = TemplateParser.Parse("images/{id}/{**path}");
        _matcher = new TemplateMatcher(template, new RouteValueDictionary());
    }

    public async Task InvokeAsync(HttpContext context, Lazy<IMediaService> mediaService,
        Lazy<IMediaAccessor> mediaAccessor, Lazy<MediaSettings> mediaSettings, ILogger<MediaMiddleware> logger)
    {
        if (!ValidatePath(context, out var id, out var remainingPath))
        {
            logger.NotProcessed();
        }

        else if (!ValidateMethod(context))
        {
            logger.MethodNotSupported(context.Request.Method);
        }

        else if (!LookupContentType(_contentTypeProvider, remainingPath, out string contentType))
        {
            logger.FormatNotSupported(remainingPath);
        }
        else
        {
            var mediaFile = await mediaService.Value.GetMediaFilesByIdAsync(id, false);
            if (mediaFile == null)
            {
                await NotFoundResponse(context, remainingPath);
                return;
            }

            var parameters = context.Request.QueryString.GetParameters();

            var mediaContext = BuildMediaContext(parameters, mediaFile, remainingPath, mediaSettings.Value);
            var result = await mediaAccessor.Value.GetMediaFile(mediaContext);
            if (result.Exists)
            {
                ApplyHttpCaching(context, mediaSettings.Value);
                await ServeStaticFileAsync(context, result, contentType);
            }

            return;
        }

        await _next(context);
    }

    private static void ApplyHttpCaching(HttpContext context, MediaSettings settings)
    {
        var headers = context.Response.Headers;


        string headerStr = string.Empty;
        switch (settings.CacheType)
        {
            case ResponseCacheLocation.Any:
                headerStr = "public, ";
                break;
            case ResponseCacheLocation.Client:
                headerStr = "private, ";
                break;
            case ResponseCacheLocation.None:
                headerStr = "no-cache, ";
                break;
        }

        var duration = settings.HttpCacheDuration >
                       0
            ? settings.HttpCacheDuration
            : 60;
        headerStr += $"max-age={duration}";


        headers.CacheControl = headerStr;
    }

    private static async Task ServeStaticFileAsync(HttpContext context, IFileInfo file, string contentType)
    {
        await using var stream = file.CreateReadStream();
        var fileResult = new FileStreamResult(stream, contentType)
        {
            EntityTag = new EntityTagHeaderValue(ETagGenerator.GenerateETag(file))
        };

        await fileResult.ExecuteResultAsync(new ActionContext
        {
            HttpContext = context,
            //RouteData = context.GetRouteData()
        });
    }


    private static async Task NotFoundResponse(HttpContext context, string remainingPath)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync(string.Format(CultureInfo.InvariantCulture, NotFound, remainingPath));
    }


    private bool ValidatePath(HttpContext ctx, out int id, out string remainingPath)
    {
        remainingPath = string.Empty;
        id = 0;
        var path = ctx.Request.Path;
        var items = new RouteValueDictionary();
        var matchResult = _matcher.TryMatch(path, items);
        if (matchResult)
        {
            if (items.TryGetAndConvertValue<int>("id", out id))
            {
                remainingPath = '/' + (string)items["path"];
                return true;
            }
        }

        return false;
    }

    private static bool ValidateMethod(HttpContext context)
        => HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method);

    private static bool LookupContentType(IContentTypeProvider contentTypeProvider, PathString subPath,
        out string? contentType) => contentTypeProvider.TryGetContentType(subPath.Value!, out contentType);

    private static MediaAccessorContext BuildMediaContext(IDictionary<string, object> p, MediaFile file,
        string remainingPath, MediaSettings mediaSettings)
    {
        var mediaQuery = new MediaAccessorContext()
        {
            Parameters = p,
            ImageDescriptor = new ImageDescriptor()
            {
                Id = file.Id,
                Path = remainingPath,
                Extension = file.MediaType,
                MaxWidth = mediaSettings.MaxImageWidth,
                MaxHeight = mediaSettings.MaxHeight
            }
        };

        return mediaQuery;
    }
}

internal static class MediaMiddlewareExtensions
{
    public static Dictionary<string, object> GetParameters(this QueryString queryString)
    {
        Guard.NotNull(queryString);
        if (!queryString.HasValue)
            return [];
        return queryString.Value!
            .TrimStart('?')
            .Split(new[] { '&', ';' }, StringSplitOptions.RemoveEmptyEntries)
            // {w=300, h=200}
            .Select(x => x.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
            // { {[w, 300]} {[h,200]} }
            .GroupBy(pair => pair[0],
                pair
                    =>
                {
                    return pair.Length > 2
                        ? string.Join('=', pair, 1, pair.Length - 1)
                        : (pair.Length > 1 ? pair[1] : string.Empty);
                })
            .ToDictionary(x => x.Key,
                x => (object)x.First());
    }
}

internal static partial class MediaLoggerExtensions
{
    [LoggerMessage(1,
        LogLevel.Debug,
        "The request doesn't match the media middleware path and will go on down the pipeline",
        EventName = "NotProcessed")]
    public static partial void NotProcessed(this ILogger logger);

    [LoggerMessage(1,
        LogLevel.Debug,
        "The request's HTTP Method {Method} isn't supported by the media middleware",
        EventName = "MethodNotSupported")]
    public static partial void MethodNotSupported(this ILogger logger, string method);

    [LoggerMessage(1,
        LogLevel.Debug,
        "The file format {Format} provided isn't supported by the media middleware",
        EventName = "FormatNotSupported")]
    public static partial void FormatNotSupported(this ILogger logger, string format);
}