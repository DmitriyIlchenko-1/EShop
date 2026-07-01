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

[HtmlTargetElement("span", Attributes = NameAttributeName)]
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
        var parameters = new Dictionary<string, object>()
        {
            { "width", 16 },
            { "height", 16 },
            { "aria-hidden", true },
            { "focusable", false },
            { "class", "icon" },
        };
        var icon = await _labelManager.GetLabelIconAsync(Name, parameters);
        output.TagName = "span";
        output.Attributes.Add("class", GetCssClassNames(Name));
        output.TagMode = TagMode.StartTagAndEndTag;
        var oldContent = await output.GetChildContentAsync();
        output.Content.Clear();
        output.Content.SetHtmlContent(icon);
        output.Content.AppendHtml(oldContent);
    }


    protected virtual string GetCssClassNames(string labelName)
    {
        string classNames = "product-card__label product-card__label--rounded ";
        switch (labelName)
        {
            case SystemLabelNames.Sale:
                classNames += "product-card__label--sale";
                break;
            default:
                classNames += "product-card__label--custom";
                break;
        }
        
        return classNames;
    }
}
 