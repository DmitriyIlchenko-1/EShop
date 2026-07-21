using System.Text.Json;
using System.Text.Json.Serialization;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;

namespace EShop.Infrastructure.Modules;

public interface IModuleDescriptor
{
    public ModuleAssemblyContext ModuleAssemblyContext { get; }
    public string SystemName { get; }
    public string FriendlyName { get; }
    public string PhysicalPath { get; }
    public string AssemblyName { get; }
}

public class ModuleDescriptor : IModuleDescriptor
{
    [JsonInclude] public string FriendlyName { get; private set; }

    public ModuleAssemblyContext ModuleAssemblyContext { get; internal set; }
    [JsonInclude] public string SystemName { get; private set; }

    [JsonInclude] public string PhysicalPath { get; private set; }

    private string _assemblyName;
    [JsonInclude]
    public string AssemblyName
    {
        get => _assemblyName ??= SystemName.EnsureEndsWith(".dll");
    }


    public static IModuleDescriptor Create(DirectoryInfo directory, string moduleRootPath)
    {
        Guard.NotNull(directory);
        if (!directory.Exists)
        {
            return null;
        }

        var manifestFile = new FileInfo(directory.FullName + Path.DirectorySeparatorChar + "module.json");
        if (!manifestFile.Exists)
        {
            return null;
        }

        var moduleDescriptor = ParseDescriptor(manifestFile);
        moduleDescriptor.SystemName ??= directory.Name;

        moduleDescriptor.PhysicalPath = directory.FullName;

     
        return moduleDescriptor;
    }

    private static ModuleDescriptor ParseDescriptor(FileInfo manifestFile)
    {
        using var stream = manifestFile.OpenRead();
        return JsonSerializer.Deserialize<ModuleDescriptor>(stream, JsonSerializerOptions.Default);
    }
}