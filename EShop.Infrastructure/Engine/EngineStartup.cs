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

public interface IEngineStartup
{
    /// <summary>
    /// Register services directly using Autofac.
    /// </summary>
    void ConfigureContainer(ContainerBuilder builder);

    /// <summary>
    /// Add services to the <see cref="IServiceCollection"/> service collection.
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    void ConfigureApplicationPipeline(IApplicationBuilder appBuilder);
}

/// <summary>
/// The purpose of this engine startup is to run all the startups without keeping them in the memory afterwards.
/// The GC will release the memory taken up by them and this object later on. 
/// </summary>
/// <typeparam name="TEngine"></typeparam>
public abstract class EngineStartup<TEngine> : IEngineStartup, IDisposable where TEngine : IEngine
{
    protected IEStartup[] _startups;
    protected IEngine _engine;

    protected EngineStartup(IEngine engine)
    {
        _engine = engine;
        ConfigureModules();
        _startups = LocateStartups()
            .OrderBy(x => x.Order)
            .ToArray();
    }

    public virtual void ConfigureContainer(ContainerBuilder builder)
    {
        // Modules ...

        foreach (var containerSetup in _startups.OfType<IContainerSetup>())
        {
            containerSetup.ConfigureContainer(builder);
        }
    }

    public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEngine>(_engine);

        var typeScanner = new DefaultTypeScanner();
        Singleton<ITypeScanner>.Instance = typeScanner;
        services.AddSingleton<ITypeScanner>(typeScanner);

        foreach (var instance in _startups)
        {
            instance.ConfigureServices(services, configuration);
        }
    }

    public virtual void ConfigureApplicationPipeline(IApplicationBuilder appBuilder)
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
        GlobalConfiguration.ContentRootPath = _engine.Environment.ContentRootPath;
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

    public override void ConfigureApplicationPipeline(IApplicationBuilder appBuilder)
    {
        foreach (var instance in _startups)
        {
            instance.ConfigureApplication(appBuilder);
        }
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        base.ConfigureServices(services, configuration);

        var mvcBuilder = services.AddControllersWithViews();

        foreach (var startup in _startups)
        {
            startup.ConfigureMvc(mvcBuilder, services);
        }
    }

    protected override void RegisterModules()
    {
        foreach (ModuleInfo moduleInfo in ModuleManager.LoadModules())
        {
            moduleInfo.Assembly = Assembly.Load(new AssemblyName(moduleInfo.Name));
            GlobalConfiguration.Modules.Add(moduleInfo);
        }
    }
}