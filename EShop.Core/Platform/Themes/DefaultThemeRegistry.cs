using System.Xml;
using EShop.Infrastructure;
using EShop.Infrastructure.FileSystem;
using Microsoft.Extensions.FileProviders;

namespace EShop.Core.Platform.Themes;

public class DefaultThemeRegistry
{
    private readonly IEShopFileProvider _fileProvider;
    protected readonly IDictionary<string, ThemeDescriptor> _themeCache = new Dictionary<string, ThemeDescriptor>();

    public DefaultThemeRegistry(IEShopFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }


    protected virtual void Initialize()
    {
        var physicalThemePath = _fileProvider.MapPath(GlobalConfiguration.ThemePath);
        foreach (var configFile in _fileProvider.EnumerableFiles(physicalThemePath))
        {
              
        }
    }
}