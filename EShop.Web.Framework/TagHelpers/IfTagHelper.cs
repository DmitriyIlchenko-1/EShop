using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EShop.Web.Common.TagHelpers;

[HtmlTargetElement("*", Attributes = IfAttributeName)]
public class IfTagHelper : TagHelper
{
    private const string IfAttributeName = "eh-if";

    public override int Order => int.MinValue;
    [HtmlAttributeName(IfAttributeName)]
    public bool Condition { get; set; } = true;
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (!Condition)
        {
            output.SuppressOutput();
        }

        return Task.CompletedTask;
    }
}