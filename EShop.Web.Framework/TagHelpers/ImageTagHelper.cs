using System.Text.Encodings.Web;
using EShop.Core.Content.Media.Configuration;
using EShop.Core.Content.Media.Services;
using EShop.Core.Platform.Web;
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
    const string SrcSetsAttributeName = "srcset";
    private const string ModelAttributeName = "es-model";
    private const string MaxSrcWidthName = "max-src-width";
    private const string MinSrcWidthName = "min-src-width";
    private const string AppendSrcsetName = "append-srcset";
    private const string ToleranceName = "tol";
    
    private readonly IMediaStorageProvider _mediaStorageProvider;
    private readonly IWebHelper _webHelper;
   
    
    public ImageTagHelper(
        IFileVersionProvider fileVersionProvider,
        HtmlEncoder htmlEncoder,
        IUrlHelperFactory urlHelperFactory, IMediaStorageProvider mediaStorageProvider, IWebHelper webHelper)
        : base(fileVersionProvider, htmlEncoder, urlHelperFactory)
    {
        _mediaStorageProvider = mediaStorageProvider;
        _webHelper = webHelper;
    }


    [HtmlAttributeName(AppendSrcsetName)] 
    public bool AppendSrcset { get; set; } = true;
    
    
    [HtmlAttributeName(MaxSrcWidthName)]
    public int MaxSrcWidth { get; set; }

    [HtmlAttributeName(MinSrcWidthName)] 
    public int MinSrcWidth { get; set; } = 300;
    [HtmlAttributeName(ToleranceName)] 
    public double Tolerance { get; set; } = .40;
    
    [HtmlAttributeName(ModelAttributeName)]
    public ImageModel Model { get; set; }
    
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Src.IsEmpty() || !AppendSrcset)
        {
            output.SuppressOutput();
            return;
        }

       
        var srcsetBuilder = new UrlBuilder(_mediaStorageProvider.Root, useHttps: _webHelper.IsCurrentConnectionSecured());
        var srcset = srcsetBuilder.BuildSrcSet(Model.Subpath, [], MinSrcWidth,
            MaxSrcWidth, Tolerance);
        output.Attributes.SetAttribute("srcset", srcset);
         
        base.Process(context, output);
    }
}