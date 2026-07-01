using System.Text.Encodings.Web;
using EShop.Core.Platform.Web;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Html;

namespace EShop.Infrastructure.Extensions;

public static class ViewExtensions
{
   public static HtmlString ToHtmlString(this IHtmlContent content)
   {
      Guard.NotNull(content);
      if (content is HtmlString str)
      {
         return str;
      }

      using var _ = StringBuilderPool.Pool.Get(out var stringBuilder);
      using var writer = new StringWriter(stringBuilder);
      content.WriteTo(writer, HtmlEncoder.Default);
      return new HtmlString(writer.ToString());
   }
}