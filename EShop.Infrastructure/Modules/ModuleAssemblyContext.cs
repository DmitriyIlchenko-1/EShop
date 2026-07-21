using System.Reflection;
using System.Runtime.Loader;
using EShop.Infrastructure.Utilities;

namespace EShop.Infrastructure.Modules;

public class ModuleAssemblyContext
{
    public ModuleAssemblyContext(IModuleDescriptor descriptor)
    {
        Guard.NotNull(descriptor);
        var assemblyPath = Path.Combine(descriptor.PhysicalPath, descriptor.AssemblyName);
        Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
    }

    public Assembly Assembly { get; set; }
}