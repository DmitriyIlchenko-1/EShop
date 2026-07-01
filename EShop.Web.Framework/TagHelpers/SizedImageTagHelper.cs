using System.Text.Encodings.Web;
 
using EShop.Core.Content.Media.Configuration;
using EShop.Core.Content.Media.Services;
using EShop.Core.Platform.Web;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Storage;
using EShop.Web.Models.Catalog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EShop.Web.Common.TagHelpers;


[HtmlTargetElement(
    "img",
    Attributes = SizesAttributeName,
    TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(
    "img",
    Attributes = MinSrcWidthName,
    TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(
    "img",
    Attributes = MaxSrcWidthName,
    TagStructure = TagStructure.WithoutEndTag)]
public class SizedImageTagHelper : TagHelper
{
    
    const string SrcAttributeName = "src";
    const string SizesAttributeName = "sizes";
    private const string MaxSrcWidthName = "max-src-width";
    private const string MinSrcWidthName = "min-src-width";
    private const string ToleranceName = "tol";
    
    private readonly IMediaStorageProvider _mediaStorageProvider;
    private readonly IWebHelper _webHelper;

    public SizedImageTagHelper(IMediaStorageProvider mediaStorageProvider, IWebHelper webHelper)
    {
        _mediaStorageProvider = mediaStorageProvider;
        _webHelper = webHelper;
    }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }
    
    [HtmlAttributeName(MaxSrcWidthName)]
    public int? MaxSrcWidth { get; set; }
    
    [HtmlAttributeName(SrcAttributeName)]
    public string Src { get; set; }

    [HtmlAttributeName(MinSrcWidthName)] 
    public int? MinSrcWidth { get; set; } 
    
    [HtmlAttributeName(ToleranceName)] 
    public double Tolerance { get; set; }
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Src.IsEmpty() || !MaxSrcWidth.HasValue || !MinSrcWidth.HasValue || MaxSrcWidth.Value < MinSrcWidth.Value)
        {
            output.CopyHtmlAttribute(SrcAttributeName, context);
            return;
        }
        
        string subpath = GetImageSubpath(Src);
        var srcsetBuilder = new UrlBuilder(ViewContext.HttpContext.Request.Host.ToString(), useHttps: _webHelper.IsCurrentConnectionSecured());
        var srcset = srcsetBuilder.BuildSrcSet(subpath, [], MinSrcWidth.Value,
            MaxSrcWidth.Value, Tolerance);
        output.Attributes.SetAttribute("srcset", srcset);
    }

    private string GetImageSubpath(string path)
    {
        return path.Substring(_mediaStorageProvider.Root.Length);
    }
}