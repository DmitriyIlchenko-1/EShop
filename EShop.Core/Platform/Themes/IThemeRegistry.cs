namespace EShop.Core.Platform.Themes;

public interface IThemeRegistry
{
    bool Contains(string themeName);
    ThemeDescriptor GetThemeByName(string themeName);
    
    /// <summary>
    /// <see href="https://stackoverflow.com/questions/24880268/ienumerable-vs-ireadonlycollection-vs-readonlycollection-for-exposing-a-list-mem"/>
    /// </summary>
    IReadOnlyList<ThemeDescriptor> GetThemeDescriptors();
  
}