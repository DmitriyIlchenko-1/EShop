using System.Xml;
using EShop.Infrastructure;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.FileSystem;
using Microsoft.Extensions.FileProviders;

namespace EShop.Core.Platform.Themes;

public class DefaultThemeRegistry : IThemeRegistry
{
    private readonly IEShopFileProvider _fileProvider;
    protected IDictionary<string, ThemeDescriptor> _themeCache;
    protected static readonly object _locker = new();

    public DefaultThemeRegistry(IEShopFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
        Initialize();
    }

    public bool Contains(string themeName)
    {
        if (themeName.IsEmpty())
            return false;

        if (_themeCache.TryGetValue(themeName, out var themeDescriptor) && themeDescriptor != null)
        {
            return true;
        }

        return false;
    }

    public ThemeDescriptor GetThemeByName(string themeName)
    {
        if (themeName.IsEmpty())
        {
            return null;
        }

        _themeCache.TryGetValue(themeName, out var theme);
        return theme;
    }

    public ICollection<ThemeDescriptor> GetThemeDescriptors()
        => _themeCache.Values.AsReadOnly();

     

    protected virtual void Initialize()
    {
        if (_themeCache != null)
            return;

        lock (_locker)
        {
            if (_themeCache != null)
                return;
            _themeCache = new Dictionary<string, ThemeDescriptor>(StringComparer.InvariantCultureIgnoreCase);
            var themePath = _fileProvider.MapPath(GlobalConfiguration.ThemePath);
            foreach (string dirName in Directory.GetDirectories(themePath))
            {
                var dir = new DirectoryInfo(dirName);
                var configFile = new FileInfo(Path.Join(dir.FullName, "theme.config"));
                if (configFile.Exists)
                {
                    var themeDescriptor = ThemeDescriptor.Create(configFile);
                    _themeCache.TryAdd(dir.Name, themeDescriptor);
                }
                else
                {
                    throw new FileNotFoundException($"Could not file theme config file for the {dirName} theme");
                }
            }
        }
    }
}