using System.Text.Encodings.Web;
using EShop.Core.Content.Media.Services;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Storage;
using EShop.Web.Models.Catalog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EShop.Web.Common.TagHelpers;

[HtmlTargetElement(
    "img",
    Attributes = SrcAttributeName,
    TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(
    "img",
    Attributes = SizesAttributeName,
    TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(
    "img",
    Attributes = SrcSetsAttributeName,
    TagStructure = TagStructure.WithoutEndTag)]
public class ImageTagHelper : Microsoft.AspNetCore.Mvc.TagHelpers.ImageTagHelper
{
    const string SrcAttributeName = "src";
    const string SizesAttributeName = "sizes";
    const string SrcSetsAttributeName = "srcsets";
    private const string ModelAttributeName = "es-model";
    private readonly IMediaStorageProvider _mediaStorageProvider;

    
    
    public ImageTagHelper(
        IFileVersionProvider fileVersionProvider,
        HtmlEncoder htmlEncoder,
        IUrlHelperFactory urlHelperFactory, IMediaStorageProvider mediaStorageProvider)
        : base(fileVersionProvider, htmlEncoder, urlHelperFactory)
    {
        _mediaStorageProvider = mediaStorageProvider;
    }


    public bool AppendSrcset { get; set; }
    public int MinSrcsetWidth { get; set; }
    public int MaxSrcsetWidth { get; set; }
    
    [HtmlAttributeName(ModelAttributeName)]
    public ImageModel Model { get; set; }
    
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Src.IsEmpty())
        {
            output.SuppressOutput();
            return;
        }

        if (AppendSrcset)
        {
            var host = "http://"+_mediaStorageProvider.Host;
            int index = Model.Url.IndexOf(host, StringComparison.Ordinal);
            string virtualPath = Model.Url.Remove(index, host.Length);
            var srcsetBuilder = new UrlBuilder(_mediaStorageProvider.Host, useHttps:false);
            var srcset = srcsetBuilder.BuildSrcSet(virtualPath, [], 300,
                MaxSrcsetWidth);
            output.Attributes.SetAttribute("srcset", srcset);
        }
        
        base.Process(context, output);
    }
}