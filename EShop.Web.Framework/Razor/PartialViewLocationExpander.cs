// using Microsoft.AspNetCore.Mvc.Razor;
// namespace EShop.Web.Common.Razor;
//
//
// /// <summary>
// /// <see href="https://stackoverflow.com/a/41435134/21915545"/>
// /// </summary>
// public class PartialViewLocationExpander : IViewLocationExpander
// {
//     private const string CacheKey = "expand-partials";
//     public void PopulateValues(ViewLocationExpanderContext context)
//     {
//         if (!context.IsMainPage && !context.ViewName.StartsWith("Components/", StringComparison.OrdinalIgnoreCase))
//         {
//             context.Values[CacheKey] = "true";
//         }
//     }
//
//     public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
//     {
//         var doExpand = context.Values.ContainsKey(CacheKey);
//         foreach (var location in viewLocations)
//         {
//             if (doExpand)
//             {
//                 yield return location.Replace("{0}", "Partials/{0}");
//             }
//
//             yield return location;
//         }
//     }
// }