using System.Reflection;

namespace EShop.Core.Platform.Infructructure.Types;

public interface ITypeScanner
{
    IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);
    IList<Assembly> GetAssemblies();
}