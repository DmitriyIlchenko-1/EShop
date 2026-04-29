using EShop.Core.Platform.Themes;
using EShop.Core.Platform.Themes.Services;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;

public interface IVirtualCssFileProcessor
{
    Task<string> GenerateOutputAsync(string request);
}

public class VirtualCssVarFileProcessor : IVirtualCssFileProcessor
{
    private readonly IThemeVariableService _themeVariableService;
    private readonly IThemeContext _themeContext;
    private const string VirtualCssVarFile = "varibles-root.css";

    public VirtualCssVarFileProcessor(IThemeVariableService themeVariableService, IThemeContext themeContext)
    {
        _themeVariableService = themeVariableService;
        _themeContext = themeContext;
    }

    public async Task<string> GenerateOutputAsync(string fileName)
    {
        Guard.NotEmpty(fileName);
        if (!fileName.Equals(VirtualCssVarFile, StringComparison.OrdinalIgnoreCase))
            return string.Empty;
        return await _themeVariableService.GenerateCssVarFile(_themeContext.WorkingThemeName);
    }
}