namespace EShop.Core.Platform.Themes;

public interface IThemeContext
{
    string WorkingThemeName { get; }
    ThemeDescriptor CurrentTheme { get; }
}