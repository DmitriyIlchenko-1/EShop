namespace EShop.Core.Platform.Themes;

public interface IThemeRegistry
{
    bool Contains(string themeName);
    ThemeDescriptor GetThemeByName(string themeName);
    ICollection<ThemeDescriptor> GetThemeDescriptors();
  
}