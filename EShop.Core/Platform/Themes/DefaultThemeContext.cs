using EShop.Core.Data;
using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Platform.Themes;

public class DefaultThemeContext : IThemeContext
{
    private readonly IThemeRegistry _registry;
    private readonly StoreGeneralSettings _settings;
    private string _cachedThemeName;
    private ThemeDescriptor _cachedCurrentTheme;

    public DefaultThemeContext(IThemeRegistry registry, StoreGeneralSettings settings)
    {
        _registry = registry;
        _settings = settings;
    }

    public string WorkingThemeName
    {
        get
        {
            if (_cachedThemeName != null)
                return _cachedThemeName;

            var theme = string.Empty;
            var defaultTheme = _settings.DefaultThemeName;
            if (!_registry.Contains(defaultTheme))
            {
                var descriptor = _registry
                    .GetThemeDescriptors()
                    .FirstOrDefault();
                if (descriptor is null)
                {
                    throw new ApplicationException("Not a single theme has been found");
                }

                theme = descriptor.ThemeName;
            }

            return _cachedThemeName = theme;
        }
    }

    public ThemeDescriptor CurrentTheme
    {
        get
        {
            if (_cachedCurrentTheme != null)
                return _cachedCurrentTheme;
            return _cachedCurrentTheme = _registry.GetThemeByName(WorkingThemeName);
        }
    }
}