using System.Diagnostics;
using System.Reflection;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using EnumerableExtensions = EShop.Infrastructure.Extensions.EnumerableExtensions;
namespace EShop.Infrastructure.Types;

public class DefaultTypeScanner : ITypeScanner
{
    public DefaultTypeScanner(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        var coreAssemblies = new HashSet<Assembly>(assemblies);
        Assemblies = EnumerableExtensions.AsReadOnly(coreAssemblies);
    }

    public IEnumerable<Assembly> Assemblies { get; private set; }
    public ILogger Logger { get; set; } = NullLogger.Instance;

    public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
    {
        return FindClassesOfTypeInternal(typeof(T), Assemblies, onlyConcreteClasses);
    }

    public IEnumerable<Type> FindClassesOfType(Type type, bool onlyConcreteClasses = true)
    {
        return FindClassesOfTypeInternal(type, Assemblies, onlyConcreteClasses);
    }

    private IEnumerable<Type> FindClassesOfTypeInternal(Type lookupType, IEnumerable<Assembly> assemblies,
        bool onlyConcreteClasses = true)
    {
        var discoveredTypes = new List<Type>();

        foreach (var a in assemblies)
        {
            IEnumerable<Type> types = a.GetLoadableTypes();

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

        return discoveredTypes;
    }
}