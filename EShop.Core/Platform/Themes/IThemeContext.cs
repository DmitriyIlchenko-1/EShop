namespace EShop.Core.Platform.Themes;

public interface IThemeContext
{
    // properties??
    string WorkingThemeName { get; }
    // properties??
    ThemeDescriptor WorkingTheme { get; }
}