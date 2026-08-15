using System.Text;
using EShop.Infrastructure.Engine;
using Newtonsoft.Json;

namespace EShop.Infrastructure.Modules;

public class ModuleManager  
{
    private const string FileName = "InstalledModules.txt";
    private static ModuleManager _instance;
    private readonly HashSet<string> _installedModules = new(StringComparer.OrdinalIgnoreCase);
    public ISet<string> InstalledModules => _installedModules;
    private readonly IApplicationContext _applicationContext;
    
    private ModuleManager()
    {
        _applicationContext = EngineContext.Current.ApplicationContext;
        if (_applicationContext == null)
        {
            throw new ArgumentNullException(nameof(_applicationContext), "Application context is null");
        }
        Initialize();
    }

    private void Initialize()
    {
        _installedModules.Clear();
        var file = _applicationContext.AppDataRoot.GetFileInfo(FileName);
        if (file.Exists)
        {
            using var stream = file.CreateReadStream();
            using var streamReader = new StreamReader(stream);
            var content = streamReader.ReadToEnd();
            var lines = File.ReadAllLines(content, Encoding.UTF8);
            foreach (var line in lines)
            {
                _installedModules.Add(line);
            }

        }
    }

    public void Save()
    {
        if (_installedModules.Count == 0)
        {
            var file = _applicationContext.AppDataRoot.GetFileInfo(FileName);
            File.Delete(file.PhysicalPath);
        }
        else
        {
            var file = _applicationContext.AppDataRoot.GetFileInfo(FileName);
            var content = string.Join(Environment.NewLine, _installedModules);
            File.WriteAllText(file.PhysicalPath, content);
        }
    }
    public static ModuleManager Instance 
        => LazyInitializer.EnsureInitialized(ref _instance, () => new ModuleManager());



}