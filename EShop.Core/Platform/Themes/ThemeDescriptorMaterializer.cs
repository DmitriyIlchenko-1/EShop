using System.Xml;
using System.Xml.Linq;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.IO;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Platform.Themes;

public class ThemeDescriptorMaterializer
{
    private readonly ThemeDescriptor _descriptor;
    private bool _isMaterialized;
    private readonly FileInfo _configFile;

    public ThemeDescriptorMaterializer(FileInfo configFile)
    {
        Guard.NotNull(configFile);
        _configFile = configFile;
        _descriptor = new ThemeDescriptor();
    }

    public ThemeDescriptor Materialize()
    {
        if (_isMaterialized)
        {
            return _descriptor;
        }
        using var read = _configFile.OpenRead();
        XElement root = XElement.Load(read);

        if (root == null)
        {
            throw new InvalidOperationException("Theme's configuration file could not be loaded.");
        }

        _descriptor.ThemeName = ((string)root.Attribute("name")).EmptyIfNull();
        _descriptor.Description = ((string)root.Attribute("description")).EmptyIfNull();
        _descriptor.Author = ((string)root.Attribute("author")).EmptyIfNull();
        _descriptor.Version = new Version(((string)root.Attribute("version")).EmptyIfNull());
        _isMaterialized = true;
        return _descriptor;
    }
}