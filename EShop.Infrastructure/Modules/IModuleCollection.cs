using System.Collections.Frozen;
using System.Reflection;
using EShop.Infrastructure.Utilities;

namespace EShop.Infrastructure.Modules;

public interface IModuleCollection
{
    IEnumerable<IModuleDescriptor> Modules { get; }
    IModuleDescriptor GetModuleByAssembly(Assembly assembly);
}


public class ModuleCollection : IModuleCollection
{
    private readonly FrozenDictionary<string, IModuleDescriptor> _nameMap;
    private readonly FrozenDictionary<Assembly, IModuleDescriptor> _assemblyMap;

    
    public ModuleCollection(IEnumerable<IModuleDescriptor> moduleDescriptors)
    {
        Guard.NotNull(moduleDescriptors);
        var nameMap = new Dictionary<string, IModuleDescriptor>(StringComparer.OrdinalIgnoreCase);
        var assemblyMap = new Dictionary<Assembly, IModuleDescriptor>();

        foreach (var descriptor in moduleDescriptors)
        {
            nameMap.Add(descriptor.SystemName, descriptor);
            assemblyMap.Add(descriptor.ModuleAssemblyContext.Assembly, descriptor);
        }

        _nameMap = nameMap.ToFrozenDictionary();
        _assemblyMap = assemblyMap.ToFrozenDictionary();
    }

    public IEnumerable<IModuleDescriptor> Modules => _nameMap.Values;

    public IModuleDescriptor GetModuleByAssembly(Assembly assembly)
    {
        if (assembly != null && _assemblyMap.TryGetValue(assembly, out var descriptor))
        {
            return descriptor;
        }

        return null;
    }
    
}