using System.Reflection;

namespace EShop.Infrastructure.Modules;

public class ModuleInfo
{
    public string Name { get; set; }
    public Version Version { get; set; }
    public bool IsBundledWithHost { get; set; }
    public Assembly Assembly { get; set; }
}