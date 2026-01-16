using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using EShop.Core.Platform.Infructructure.Types;

namespace EShop.Infrastructure.Types;

public class DefaultTypeScanner : ITypeScanner
{
    protected static bool _assembliesLoaded;
    protected static readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();
    protected static readonly object _locker = new object();

    protected const string AssemblySkipLoadingPattern =
        "^System|^Anonymous|^ef|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antrl3|^Autofac|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease";

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
                        if (!GetClosedGenericTypesOf(type, lookupType)
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

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName == null)
            {
                continue;
            }

            if (assembly.FullName.Contains("EShop"))
            {
                Console.WriteLine();
            }

            if (!Matches(assembly.FullName))
            {
                continue;
            }

            _assemblies.TryAdd(assembly.FullName, assembly);
        }

        _assembliesLoaded = true;
    }


    protected static bool Matches(string assemblyFullName)
    {
        return !Regex.IsMatch(assemblyFullName,
            AssemblySkipLoadingPattern,
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    private static IEnumerable<Type> GetClosedGenericTypesOf(Type source, Type openGeneric)
    {
        if (!openGeneric.IsGenericTypeDefinition)
        {
            return Enumerable.Empty<Type>();
        }

        return GetTypesAssignableFrom(source)
            .Where(t => !t.ContainsGenericParameters && t.IsGenericType && t.GetGenericTypeDefinition() == openGeneric);
    }

    private static IEnumerable<Type> GetTypesAssignableFrom(Type source)
    {
        var interfaces = source.GetInterfaces();
        foreach (var _interface in interfaces)
        {
            yield return _interface;
        }

        while (source != null && source != typeof(object))
        {
            yield return source;
            source = source.BaseType;
        }
    }
}