using Autofac;

namespace EShop.Infrastructure.Modules;

public interface IContainerSetup
{
    void ConfigureContainer(ContainerBuilder builder);
}