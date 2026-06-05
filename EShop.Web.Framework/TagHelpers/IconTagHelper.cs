using System.Text;
using System.Xml;
using System.Xml.Linq;
using AngleSharp;
using Aspose.Svg;
using Aspose.Svg.Rendering;
using Aspose.Svg.Rendering.Pdf;
using EShop.Core.Common.Domain;
using EShop.Core.Common.Services;
using EShop.Core.Data;
using EShop.Core.Platform.Caching;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace EShop.Web.Common.TagHelpers;

[HtmlTargetElement("svg", Attributes = NameAttributeName)]
public class IconTagHelper : TagHelper
{
    private const string NameAttributeName = "name";
    [HtmlAttributeName(NameAttributeName)] public string Name { get; set; }

    public IconTagHelper(ILabelManager labelManager)
    {
        _labelManager = labelManager;
    }

    private readonly ILabelManager _labelManager;


    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var parameters = new Dictionary<string, object>();
        var icon = await _labelManager.GetLabelIconAsync(Name, parameters);
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(icon);
    }
}
 