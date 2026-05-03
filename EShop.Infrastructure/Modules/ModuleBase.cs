namespace EShop.Infrastructure.Modules;

public abstract class ModuleBase : IModule
{
    public IModuleDescriptor ModuleDescriptor { get; set; }

    public async Task InstallAsync()
    {
        await InstallCoreAsync();
        ModuleManager.Instance.InstalledModules.Add(ModuleDescriptor.Name);
        ModuleManager.Instance.Save();
    }

    protected virtual Task InstallCoreAsync()
    {
        return Task.CompletedTask;
    }

    public async Task UninstallAsync()
    {
        await UninstallCoreAsync();
        ModuleManager.Instance.InstalledModules.Remove(ModuleDescriptor.Name);
        ModuleManager.Instance.Save();
    }
    
    protected virtual Task UninstallCoreAsync()
    {
        return Task.CompletedTask;
    }
}


public interface IModule
{
    public IModuleDescriptor ModuleDescriptor { get; set; }
    Task InstallAsync();
    Task UninstallAsync();
}