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

     
    public string ThemeName { get; internal set; }
    public string Description { get; internal set; }
    public string Author { get; internal set; }
    public Version Version { get; internal set; }
    public string PhysicalPath { get; internal set; }
    public IDictionary<string, ThemeVariableMetadata> Variables { get; internal set; }
}