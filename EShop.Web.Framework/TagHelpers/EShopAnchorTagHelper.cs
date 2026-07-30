// using EShop.Infrastructure.Extensions;
// using Microsoft.AspNetCore.Mvc.TagHelpers;
// using Microsoft.AspNetCore.Mvc.ViewFeatures;
// using Microsoft.AspNetCore.Razor.TagHelpers;
//
// namespace EShop.Web.Common.TagHelpers;
//
// [HtmlTargetElement("a", Attributes = HighlightWithClassName)]
// public class EShopAnchorTagHelper : AnchorTagHelper
// {
//     private const string HighlightWithClassName = "es-highlight-with";
//     
//     [HtmlAttributeName(HighlightWithClassName)]
//     public string HighlightActiveClassName { get; set; }
//     
//     public EShopAnchorTagHelper(IHtmlGenerator generator) : base(generator)
//     {
//     }
//
//     public override void Process(TagHelperContext context, TagHelperOutput output)
//     {
//         bool matches = false;
//         string contextController =
//             this.ViewContext.RouteData.Values.GetValueOrDefaultAs<string>("controller");
//         string contextActions =
//             this.ViewContext.RouteData.Values.GetValueOrDefaultAs<string>("action");
//
//         if (this.Controller.Equals(contextController, StringComparison.InvariantCultureIgnoreCase)
//             && this.Action.Equals(contextActions, StringComparison.InvariantCultureIgnoreCase))
//         {
//             matches = true;
//         }
//
//         if (matches)
//         {
//             output.Attributes.Add("class", HighlightActiveClassName);
//         }
//         base.Process(context, output);
//     }
// }