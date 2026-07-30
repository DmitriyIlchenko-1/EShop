using System.Reflection;
using System.Runtime.Loader;
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
/// Abstract base class also engages the template pattern to facilitate code reuse. 
/// </summary>
/// <remarks>
/// The purpose of this engine startup is to run all the startups without keeping them in the memory afterwards.
/// The GC will release the memory taken up by them and this object later on.
/// </remarks>
public abstract class EngineStartup<TEngine> : Disposable, IEngineStartup where TEngine : IEngine
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
        foreach (var containerSetup in _startups.OfType<IContainerSetup>())
        {
            containerSetup.ConfigureContainer(builder, Engine.ApplicationContext);
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


    protected abstract IEnumerable<Assembly> ResolveCoreAssemblies();

    private void ConfigureModules()
    {
        var coreAssemblies = ResolveCoreAssemblies();
        Singleton<ITypeScanner>.Instance = new DefaultTypeScanner(coreAssemblies);
        var modules = FindAllModules();

        foreach (var module in modules)
        {
            LoadModule(module as ModuleDescriptor);
        }

        var moduleCollection = new ModuleCollection(modules);
        Engine.ApplicationContext.ModuleCollection = moduleCollection;
        Singleton<ITypeScanner>.Instance = new DefaultTypeScanner(coreAssemblies, moduleCollection);

        
        void LoadModule(ModuleDescriptor descriptor)
        {
            descriptor.ModuleAssemblyContext = new ModuleAssemblyContext(descriptor);
        }
    }

    protected virtual IEnumerable<IModuleDescriptor> FindAllModules()
    {
        return [];
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
        services.AddSingleton<IApplicationContext>(Engine.ApplicationContext);

        foreach (var startup in Startups)
        {
            startup.ConfigureMvc(mvcBuilder, services);
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

    protected override IEnumerable<IModuleDescriptor> FindAllModules()
    {
        var allDirectories = Engine.ApplicationContext.ModuleRoot.GetDirectoryContents("/");
        var modules = allDirectories
            .Where(x => x.IsDirectory)
            .Select(x => new DirectoryInfo(x.PhysicalPath))
            .Select(x => ModuleDescriptor.Create(x, x.Parent!.FullName))
            .Where(x => x != null);

        return modules.ToArray();
    }
}