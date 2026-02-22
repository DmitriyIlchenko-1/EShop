using System.Reflection;
using System.Runtime.Loader;
using Autofac;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Types;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

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
/// The purpose of this engine startup is to run all the startups without keeping them in the memory afterwards.
/// The GC will release the memory taken up by them and this object later on. 
/// </summary>
/// <typeparam name="TEngine"></typeparam>
public abstract class EngineStartup<TEngine> : Disposable, IEngineStartup where TEngine : IEngine
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
        foreach (var containerSetup in _startups.OfType<IContainerSetup>())
        {
            containerSetup.ConfigureContainer(builder);
        }
    }

    public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEngine>(_engine);
        services.AddSingleton<ITypeScanner>(Singleton<ITypeScanner>.Instance);


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


    protected abstract IEnumerable<Assembly> ResolveCoreAssemblies();

    private void ConfigureModules()
    {
        var coreAssemblies = ResolveCoreAssemblies();
        var typeScanner = new DefaultTypeScanner(coreAssemblies);
        Singleton<ITypeScanner>.Instance = typeScanner;
        RegisterModules();
    }

    protected virtual void RegisterModules()
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _engine = null;
            _startups = null;
        }
    }
}

public class EShopEngineStartup : EngineStartup<EShopEngine>
{
    private const string AssemblyPrefix = "EShop";

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

    protected override IEnumerable<Assembly> ResolveCoreAssemblies()
    {
        var assemblies = new HashSet<Assembly>();

        //This is what lets us access the dependencies used to compile the app without relying on lazy assembly loading. 
        var libraries = DependencyContext
            .Default
            .CompileLibraries
            .Where(x => IsCoreAssembly(x.Name))
            .Select(x => new
            {
                Name = new AssemblyName(x.Name),
            });

        // Retrieve the assemblies that have already been loaded so we don't have to do that again.
        var appAssemblies = AssemblyLoadContext
            .Default.Assemblies
            .Where(x => x.FullName.StartsWith(AssemblyPrefix) && IsCoreAssembly(x.GetName()
                .Name))
            .Select(x => new
            {
                Name = x.GetName(),
                Assembly = x
            });
        foreach (var lib in libraries)
        {
            try
            {
                var loadedAssembly = appAssemblies.FirstOrDefault(x => x.Name.Name == lib.Name.Name)
                    ?.Assembly;
                if (loadedAssembly is null)
                {
                    loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyName(lib.Name);
                }

                if (loadedAssembly is not null)
                {
                    assemblies.Add(loadedAssembly);
                }
            }
            catch (Exception e)
            {
                //todo: log exceptions in here
            }
        }

        return assemblies;

        bool IsCoreAssembly(string name)
        {
            return name == AssemblyPrefix || name.StartsWith(AssemblyPrefix);
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
        GlobalConfiguration.ContentRootPath = _engine.Environment.ContentRootPath;

        foreach (ModuleInfo moduleInfo in ModuleManager.LoadModules())
        {
            moduleInfo.Assembly = Assembly.Load(new AssemblyName(moduleInfo.Name));
            GlobalConfiguration.Modules.Add(moduleInfo);
        }
    }
}