using Newtonsoft.Json;

namespace EShop.Infrastructure.Modules;

public static class ModuleManager  
{
    private static readonly string ModulesFilename = "modules.json";

    public static IEnumerable<ModuleInfo> LoadModules()
    {
        string modulesPath = Path.Combine(GlobalConfiguration.ContentRootPath, ModulesFilename);
        using StreamReader reader = new StreamReader(modulesPath);
        string content = reader.ReadToEnd();
        dynamic modulesData = JsonConvert.DeserializeObject(content);
        foreach (dynamic module in modulesData)
        {
            yield return new ModuleInfo
            {
                Name = module.name,
                IsBundledWithHost = module.isBundledWithHost,
                Version = Version.Parse(module.version.ToString()),
            };
        }

    }
}