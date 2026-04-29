namespace EShop.Core.Platform.Themes.Domain;

public enum ThemeVariableType
{
    Color,
    Integer,
    String, 
    Boolean
}

public class ThemeVariableMetadata
{
    public string Name { get; set; }

    public ThemeVariableType Type { get; set; }

    public string DefaultValue { get; set; }
}