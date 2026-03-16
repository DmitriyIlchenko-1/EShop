using EShop.Core.Platform.Themes;
using EShop.Web.Common.Routing;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Razor;

/// <summary>
/// <see href="https://stackoverflow.com/a/41435134/21915545"/>
/// </summary>
public class ThemeLocationExpander : IViewLocationExpander
{
    private const string CacheKey = "WorkingTheme";
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        if (context.AreaName?.Equals(AreaConstValue.Admin) ?? false)
            return;
        var themeContext = context.ActionContext.HttpContext.RequestServices.GetRequiredService<IThemeContext>();
        context.Values[CacheKey] = themeContext.WorkingThemeName;
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
        IEnumerable<string> viewLocations)
    {
        if (context.Values.TryGetValue(CacheKey, out var themeName))
        {
            viewLocations = viewLocations.Concat(new[]
            {
                $"Themes/{themeName}/Views/{{1}}/{{0}}.cshtml",
                $"Themes/{themeName}/Views/Shared/{{0}}.cshtml"
            });
        }

        return viewLocations;
    }
}