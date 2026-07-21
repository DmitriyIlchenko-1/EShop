using Autofac;
using EShop.Infrastructure.Engine;

namespace EShop.Infrastructure.Modules;

public interface IContainerSetup
{
    void ConfigureContainer(ContainerBuilder builder,IApplicationContext applicationContext);
}