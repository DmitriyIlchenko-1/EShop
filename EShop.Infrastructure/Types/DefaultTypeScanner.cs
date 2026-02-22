using System.Diagnostics;
using System.Reflection;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Extensions;

namespace EShop.Infrastructure.Types;

public class DefaultTypeScanner : ITypeScanner
{
    
    protected const string AssemblyPrefix = "EShop";

    public DefaultTypeScanner(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        var coreAssemblies = new HashSet<Assembly>(assemblies);
        Assemblies = coreAssemblies.AsReadOnly();
    }

    public IEnumerable<Assembly> Assemblies { get; private set; }

    public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
    {
        return FindClassesOfType(typeof(T), Assemblies, onlyConcreteClasses);
    }

    public IEnumerable<Type> FindClassesOfType(Type type, bool onlyConcreteClasses = true)
    {
        return FindClassesOfType(type, Assemblies, onlyConcreteClasses);
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
    
    protected static bool IsCoreAssembly(string name)
    {
        return name == AssemblyPrefix || name.StartsWith(AssemblyPrefix);
    }
}