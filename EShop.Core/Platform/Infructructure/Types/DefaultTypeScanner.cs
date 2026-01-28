using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Policy;
using System.Text.RegularExpressions;
using EShop.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyModel;

namespace EShop.Core.Platform.Infructructure.Types;

public class DefaultTypeScanner : ITypeScanner
{
    protected static bool _assembliesLoaded;
    protected static readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();
    protected static readonly object _locker = new object();

    // protected const string AssemblySkipLoadingPattern =
    //     "^System|^Anonymous|^ef|^mscorlib|^Microsoft|^AjaxControlToolkit|^Npgsql|^ZiggyCreatures|^StackExchange|^Pipelines|^Antrl3|^Autofac|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease";

    protected const string AssemblyPrefix = "EShop";

    public IList<Assembly> GetAssemblies()
    {
        if (!_assembliesLoaded)
        {
            Initialize();
        }

        return _assemblies.Values.ToList();
    }

    public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
    {
        return FindClassesOfType(typeof(T), GetAssemblies(), onlyConcreteClasses);
    }

    public IEnumerable<Type> FindClassesOfType(Type type, bool onlyConcreteClasses = true)
    {
        return FindClassesOfType(type, GetAssemblies(), onlyConcreteClasses);
    }

    protected IEnumerable<Type> FindClassesOfType(Type lookupType, IEnumerable<Assembly> assemblies,
        bool onlyConcreteClasses = true)
    {
        var discoveredTypes = new List<Type>();

        try
        {
            foreach (var a in assemblies)
            {
                Type[] types = null;
                try
                {
                    types = a.GetTypes();
                }
                catch (Exception e)
                {
                }

                if (types == null)
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (!lookupType.IsAssignableFrom(type))
                    {
                        continue;
                    }

                    if (type.IsInterface)
                    {
                        continue;
                    }

                    var isOpenGeneric = lookupType.IsGenericTypeDefinition;

                    if (isOpenGeneric)
                    {
                        if (!type
                                .GetClosedGenericTypesOf(lookupType)
                                .Any())
                        {
                            continue;
                        }
                    }

                    if (onlyConcreteClasses)
                    {
                        if (type.IsClass && !type.IsAbstract)
                        {
                            discoveredTypes.Add(type);
                        }
                    }
                    else
                    {
                        discoveredTypes.Add(type);
                    }
                }
            }
        }
        catch (ReflectionTypeLoadException e)
        {
            var msg = string.Empty;
            if (e.LoaderExceptions.Any())
            {
                msg = e
                    .LoaderExceptions.Where(x => x != null)
                    .Aggregate(msg, (current, e) => $"{current}{e.Message + Environment.NewLine}");
            }

            var rethrow = new Exception(msg, e);
            Debug.WriteLine(rethrow.Message, rethrow);
            throw rethrow;
        }

        return discoveredTypes;
    }

    private void Initialize()
    {
        if (_assembliesLoaded)
        {
            return;
        }

        lock (_locker)
        {
            if (_assembliesLoaded)
            {
                return;
            }
        }

        // var rootAssembly = Assembly.GetEntryAssembly();
        // var visited = new HashSet<string>();
        // var queue = new Queue<Assembly>();
        // queue.Enqueue(rootAssembly);
        // while (queue.Count > 0)
        // {
        //     var assembly = queue.Dequeue();
        //     visited.Add(assembly.FullName);
        //     var references = assembly.GetReferencedAssemblies();
        //
        //     foreach (var reference in references)
        //     {
        //         var fullName = reference.FullName;
        //         if (fullName == null || visited.Contains(fullName) || !IsCoreAssembly(fullName))
        //         {
        //             continue;
        //         }
        //
        //         var assemblyObj = Assembly.Load(reference.FullName);
        //         queue.Enqueue(assemblyObj);
        //         _assemblies.TryAdd(fullName, assemblyObj);
        //     }
        // }

        var coreAssemblies = DependencyContext
            .Default.CompileLibraries.Where(x => IsCoreAssembly(x.Name))
            .ToList();

        coreAssemblies.ForEach(x => { _assemblies.TryAdd(x.Name, Assembly.Load(x.Name)); });


        _assembliesLoaded = true;
    }


    protected static bool IsCoreAssembly(string name)
    {
        return name == AssemblyPrefix || name.StartsWith(AssemblyPrefix);
    }
}