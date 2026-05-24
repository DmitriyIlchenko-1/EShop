using System.Collections.Concurrent;
using System.Xml;
using EShop.Infrastructure;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Extensions;
 
using Microsoft.Extensions.FileProviders;

namespace EShop.Core.Platform.Themes;

public class DefaultThemeRegistry : IThemeRegistry
{
    private readonly IFileProvider _fileProvider;
    private IDictionary<string, ThemeDescriptor> _themeCache;
    private static readonly object _locker = new();

    public DefaultThemeRegistry(IApplicationContext app)
    {
        //todo: change to theme provider
        _fileProvider = app.WebRoot;
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

    public IReadOnlyList<ThemeDescriptor> GetThemeDescriptors()
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

            var dirPaths = _fileProvider
                .GetDirectoryContents(GlobalConfiguration.ThemePath)
                .Where(x => x.IsDirectory)
                .Select(x => x.PhysicalPath);
            foreach (string dirPath in dirPaths)
            {
                var dir = new DirectoryInfo(dirPath);
                var configFile = new FileInfo(Path.Join(dir.FullName, "theme.config"));
                if (configFile.Exists)
                {
                    var themeDescriptor = ThemeDescriptor.Create(configFile);
                    _themeCache.TryAdd(dir.Name, themeDescriptor);
                }
                else
                {
                    throw new FileNotFoundException($"Could not file theme config file for the {dirPath} theme");
                }
            }
        }
    }
}