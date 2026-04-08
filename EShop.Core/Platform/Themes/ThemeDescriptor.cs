using EShop.Core.Platform.Themes.Domain;
using EShop.Infrastructure.IO;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Platform.Themes;

public class ThemeDescriptor
{
    // I think that if it was  a real project designed to be used in production,
    // then we would have to create a separate type representing the description configuration
    // rather than passing the file directly to the method so that later
    // on if we wanted to add anything on top of the XML configuration we could 
    // do this without having to modify the parameter type.
    internal static ThemeDescriptor Create(FileInfo themeConfiguration)
    {
        Guard.NotNull(themeConfiguration);
        var materializer = new ThemeDescriptorMaterializer(themeConfiguration);
        return materializer.Materialize();

    }

     //TODO: 
    public string ThemeName { get;   set; }
    public string Description { get;   set; }
    public string Author { get;   set; }
    public Version Version { get;   set; }
    public string PhysicalPath { get; internal set; }
    public IDictionary<string, ThemeVariableMetadata> Variables { get; internal set; }

    public override bool Equals(object obj)
    {
        var other = obj as ThemeDescriptor;
        return other != null &&
               ThemeName.Equals(other.ThemeName, StringComparison.InvariantCulture);
    }

    //TODO: 
    public override int GetHashCode()
    {
        return ThemeName.GetHashCode();
    }
}