using EShop.Infrastructure.Modules;
 
namespace EShop.Infrastructure;

public static class GlobalConfiguration
{
    public static List<ModuleInfo> Modules { get; set; } = new List<ModuleInfo>();
    public static string ContentRootPath { get; set; }

    public const string ThemePath = "Themes";

    public static readonly Type[] IgnoredInterfaces = new[]
    {
        typeof(IDisposable),
        typeof(IAsyncDisposable),
    };
}

