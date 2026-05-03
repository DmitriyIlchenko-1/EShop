namespace EShop.Infrastructure.Modules;

public interface IModuleDescriptor
{
    public string Name { get; set; }
    public string AssemblyName { get; set; }
   
}