using System.Reflection;

namespace EShop.Core.Platform.Infructructure.Types;

public interface ITypeScanner
{
    IEnumerable<Type> FindClassesOfType(Type type, bool onlyConcreteClasses = true);
    IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);
    public IEnumerable<Assembly> Assemblies { get; }
}