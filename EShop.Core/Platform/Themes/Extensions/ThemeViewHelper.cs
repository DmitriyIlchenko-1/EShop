using System.ComponentModel;
using System.Dynamic;
using EShop.Core.Platform.Themes.Services;
using EShop.Infrastructure.Common;
using EShop.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Core.Platform.Themes.Extensions;

public static class ThemeViewHelper
{
    public static ThemeDescriptor GetThemeDescriptor(this IViewHelper viewHelper)
        => viewHelper.HttpContext.RequestServices.GetRequiredService<IThemeContext>()
            .WorkingTheme;

    public static dynamic GetThemeVariables(this IViewHelper viewHelper)
    {
        return viewHelper.HttpContext.GetItem("ThemeVariables",
            () =>
            {
                var services = viewHelper.HttpContext.RequestServices;
                var themeDescriptor = viewHelper.GetThemeDescriptor();
                if (themeDescriptor is null)
                {
                    return new ExpandoObject();
                }

                return services
                    .GetRequiredService<IThemeVariableService>()
                    .GetThemeVariablesAsync(themeDescriptor.ThemeName)
                    .GetAwaiter()
                    .GetResult();
            });
    }

    public static T GetVariable<T>(this IViewHelper viewHelper, string name)
    {
        var vars = viewHelper.GetThemeVariables() as IDictionary<string, object>;
        if (vars.TryGetValueAs<string>(name, out string result) && !result.IsEmpty())
        {
           return (T)Convert.ChangeType(result, typeof(T));
        }

        return default(T);
    }
}