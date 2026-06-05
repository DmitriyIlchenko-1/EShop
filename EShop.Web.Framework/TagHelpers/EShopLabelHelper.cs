using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EShop.Web.Common.TagHelpers;


[HtmlTargetElement(Attributes = LabelAttributeName)]
public class EShopLabelHelper : TagHelper
{
    private const string LabelAttributeName = "es-label";


    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return base.ProcessAsync(context, output);
    }
}