// using EShop.Core.Platform.Themes;
// using Microsoft.AspNetCore.Mvc.Razor;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace EShop.Web.Common.Razor;
//
// /// <summary>
// /// <see href="https://stackoverflow.com/a/41435134/21915545"/>
// /// </summary>
// public class ThemeLocationExpander : IViewLocationExpander
// {
//     private const string CacheKey = "WorkingTheme";
//    
//
//     public ThemeLocationExpander(IThemeContext themeContext)
//     {
//        
//     }
//
//     public void PopulateValues(ViewLocationExpanderContext context)
//     {
//         var themeContext = context.ActionContext.HttpContext.RequestServices.GetRequiredService<IThemeContext>();
//         context.Values[CacheKey] = themeContext.WorkingThemeName;
//     }
//
//     public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
//         IEnumerable<string> viewLocations)
//     {
//         if (context.Values.TryGetValue(CacheKey, out var themeName))
//         {
//             var registry = context.ActionContext.HttpContext.RequestServices.GetRequiredService<IThemeRegistry>();
//             var currentTheme = registry.GetThemeByName(themeName);
//             
//              
//         }
//     }
// }