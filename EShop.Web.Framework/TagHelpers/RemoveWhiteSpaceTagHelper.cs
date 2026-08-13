using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EShop.Web.Common.TagHelpers;

[HtmlTargetElement("*", Attributes = RemoveWhiteSpaceName)]

public class RemoveWhiteSpaceTagHelper : TagHelper
{
    private const string RemoveWhiteSpaceName = "remove-white-space";
    public override int Order => int.MaxValue;
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);
        var content = (await output.GetChildContentAsync()).GetContent();
        content = content.Trim();
        output.Content.SetHtmlContent(content);
    }
}