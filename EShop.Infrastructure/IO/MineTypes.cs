using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.StaticFiles;

namespace EShop.Infrastructure.IO;

public static class MimeTypes
{
     private static readonly IContentTypeProvider ContentTypeProvider = new FileExtensionContentTypeProvider();


     public static bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType)
     {
          return ContentTypeProvider.TryGetContentType(subpath, out contentType);
     }

}