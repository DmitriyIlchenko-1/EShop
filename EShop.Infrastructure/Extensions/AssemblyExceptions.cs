using System.Reflection;

namespace EShop.Infrastructure.Extensions;

public static class AssemblyExceptions
{
    /// <see href="https://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx/"/>
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(x => x != null);
        }
    }
}