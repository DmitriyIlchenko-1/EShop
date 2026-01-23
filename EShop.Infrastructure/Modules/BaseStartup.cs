using Autofac;
using EShop.Infrastructure.Engine;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Infrastructure.Modules;

public abstract class BaseStartup : IEStartup, IContainerSetup
{
    public virtual int Order { get; } = PipelineOrder.Default;

    public virtual void ConfigureApplication(IApplicationBuilder app)
    {
    }

    public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public virtual void ConfigureContainer(ContainerBuilder builder)
    {
    }
}