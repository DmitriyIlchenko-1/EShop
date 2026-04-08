using System.Xml;
using System.Xml.Linq;
using EShop.Core.Platform.Themes.Domain;
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
        _descriptor.PhysicalPath = _configFile.FullName;
        _descriptor.Variables = MaterializeVariables(root);
        _isMaterialized = true;
        return _descriptor;
    }

    private IDictionary<string, ThemeVariableMetadata> MaterializeVariables(XElement root)
    {
        var dict = new Dictionary<string, ThemeVariableMetadata>(StringComparer.InvariantCultureIgnoreCase);
        var variablesElem = root.Element("Variables");
        if (variablesElem == null)
            return dict;
        foreach (var xElement in variablesElem.Elements("Var"))
        {
           var meta = MaterializeVariable(xElement);
            if (!dict.TryAdd(meta.Name, meta))
                throw new InvalidOperationException($"Variable {xElement} is duplicated in the configuration file '{_descriptor.PhysicalPath}'.");
        }

        return dict;
    }

    private ThemeVariableMetadata MaterializeVariable(XElement xElement)
    {
        var name = xElement.Attribute("name")
            ?.Value;
        if (name.IsEmpty())
            throw new InvalidOperationException($"Variable {xElement} has no defined name in the configuration file '{_descriptor.PhysicalPath}'.");
        var result = Enum.TryParse<ThemeVariableType>(xElement.Attribute("type")
                ?.Value, ignoreCase:true,
            out var type);
        if (!result)
            throw new InvalidOperationException($"Variable {xElement} doesn't have a type in the configuration file '{_descriptor.PhysicalPath}'.");

        var metadata = new ThemeVariableMetadata()
        {
            Name = name,
            DefaultValue = xElement.Value.EmptyIfNull(),
            Type = type,
        };
        return metadata;
    }
}