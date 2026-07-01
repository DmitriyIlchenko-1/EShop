using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EShop.Web.Common.TagHelpers;

[HtmlTargetElement("span", Attributes = "asp-validation-for")]
public class ValidationTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
       output.Attributes.SetAttribute("role", "alert");
    }
}