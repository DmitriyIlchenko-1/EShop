using EShop.Core.Data;
using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Platform.Themes;

public class DefaultThemeContext : IThemeContext
{
    protected readonly IThemeRegistry _registry;
    protected readonly StoreGeneralSettings _settings;
    protected string _cachedThemeName;
     

    public DefaultThemeContext(IThemeRegistry registry, StoreGeneralSettings settings)
    {
        _registry = registry;
        _settings = settings;
    }

    public virtual string WorkingThemeName
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

   
}