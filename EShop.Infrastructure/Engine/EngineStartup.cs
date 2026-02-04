using System.Collections.ObjectModel;
using System.Reflection;
using Autofac;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Infrastructure.Engine;

public interface IEngineStartup : IDisposable
{
    /// <summary>
    /// Register services directly using Autofac.
    /// </summary>
    void ConfigureContainer(ContainerBuilder builder);

    /// <summary>
    /// Add services to the <see cref="IServiceCollection"/>  
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    void ConfigureApplicationPipeline(IApplicationBuilder appBuilder);
}

/// <summary>
/// Abstract base class also engages the template pattern to facilitate code reuse. 
/// </summary>
/// <remarks>
/// The purpose of this engine startup is to run all the startups without keeping them in the memory afterwards.
/// The GC will release the memory taken up by them and this object later on.
/// </remarks>
public abstract class EngineStartup<TEngine> : IEngineStartup where TEngine : IEngine
{
    // Protected members used to expose advanced customization options (in case of subclassing) without unnecessarily complication the main public interface.
    protected IEStartup[] Startups => _startups;
    protected IEngine Engine => _engine;

    private IEngine _engine;
    private IEStartup[] _startups;
 
    // The ctor acts as a template method.
    protected EngineStartup(IEngine engine)
    {
        _engine = engine;
        ConfigureModules();
        _startups = LocateStartups()
            .OrderBy(x => x.Order)
            .ToArray();
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        // Modules ...

        foreach (var containerSetup in _startups.OfType<IContainerSetup>())
        {
            containerSetup.ConfigureContainer(builder);
        }

        ConfigureContainerCore(builder);
    }

    protected virtual void ConfigureContainerCore(ContainerBuilder builder)
    {
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEngine>(_engine);
        services.AddSingleton<ITypeScanner>(Singleton<ITypeScanner>.Instance);

        foreach (var instance in _startups)
        {
            instance.ConfigureServices(services, configuration);
        }

        ConfigureServicesCore(services, configuration);
    }

    protected virtual void ConfigureServicesCore(IServiceCollection services, IConfiguration configuration)
    {
    }

    // We still follow the template pattern in case we ever need to do something to the base class method.
    // Not every EngineStartup implementation needs or have anything to register in the pipeline. The standard HTTP pipeline will do for them.
    // ConfigureApplicationPipelineCore is a hook subclasses that do need to configure the pipeline can override to do that.
    // It means: 'It case a subclass cares about configuring the pipeline - they can override the hook, though it's optional.
    public void ConfigureApplicationPipeline(IApplicationBuilder appBuilder)
        => ConfigureApplicationPipelineCore(appBuilder);

    protected virtual void ConfigureApplicationPipelineCore(IApplicationBuilder appBuilder)
    {
    }


    protected virtual IEnumerable<IEStartup> LocateStartups()
    {
        var typeScanner = Singleton<ITypeScanner>.Instance;
        var startups = typeScanner.FindClassesOfType<IEStartup>();

        var instances = startups
            .Select(s => (IEStartup)Activator.CreateInstance(s))
            .Where(s => s != null);

        return instances;
    }


    private void ConfigureModules()
    {
        var typeScanner = new DefaultTypeScanner();
        Singleton<ITypeScanner>.Instance = typeScanner;
        RegisterModules();
    }

    protected virtual void RegisterModules()
    {
    }

    public void Dispose()
    {
        _engine = null;
        _startups = null;
    }
}

public class EShopEngineStartup : EngineStartup<EShopEngine>
{
    public EShopEngineStartup(IEngine engine) : base(engine)
    {
    }

    protected override void ConfigureApplicationPipelineCore(IApplicationBuilder appBuilder)
    {
        foreach (var instance in Startups)
        {
            instance.ConfigureApplication(appBuilder);
        }
    }

    protected override void ConfigureServicesCore(IServiceCollection services, IConfiguration configuration)
    {
        var mvcBuilder = services.AddControllersWithViews();

        foreach (var startup in Startups)
        {
            startup.ConfigureMvc(mvcBuilder, services);
        }
    }

    protected override void RegisterModules()
    {
        GlobalConfiguration.ContentRootPath = Engine.Environment.ContentRootPath;

        foreach (ModuleInfo moduleInfo in ModuleManager.LoadModules())
        {
            moduleInfo.Assembly = Assembly.Load(new AssemblyName(moduleInfo.Name));
            GlobalConfiguration.Modules.Add(moduleInfo);
        }
    }
}

 