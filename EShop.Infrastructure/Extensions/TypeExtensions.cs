using System.Reflection;

namespace EShop.Infrastructure.Extensions;

public static class TypeExtensions
{
    public static TAttribute GetSingleAttribute<TAttribute>(this ICustomAttributeProvider provider, bool inherits)
        where TAttribute : Attribute
    {
        if (provider.IsDefined(typeof(TAttribute), inherits))
        {
            var attributes = provider.GetCustomAttributes(typeof(TAttribute), inherits);
            if (attributes.Length > 0)
            {
                throw new InvalidOperationException("Entity type has more than one instance of the attribute");
            }

            return (TAttribute)attributes[0];
        }

        return null;
    }

    public static bool IsBasicOrNullableType(this Type t)
    {
        return t.IsBasicType() || Nullable.GetUnderlyingType(t) != null;
    }


    public static bool IsBasicType(this Type t)
    {
        return t.IsPrimitive
               || t.IsEnum
               || t == typeof(string)
               || t == typeof(decimal)
               || t == typeof(DateTime)
               || t == typeof(TimeSpan)
               || t == typeof(Guid)
               || t == typeof(byte[]);
    }

    public static IEnumerable<Type> GetClosedGenericTypesOf(this Type source, Type openGeneric)
    {
        if (!openGeneric.IsGenericTypeDefinition)
        {
            return Enumerable.Empty<Type>();
        }

        return GetTypesAssignableFrom(source)
            .Where(t => !t.ContainsGenericParameters && t.IsGenericType && t.GetGenericTypeDefinition() == openGeneric);
    }

    public static bool IsOpenGeneric(this Type type)
    {
        return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
    }


    public static TAttribute GetSingleAttribute<TAttribute>(this Type type, bool inherits) where TAttribute : Attribute
    {
        if (type.IsDefined(typeof(TAttribute), inherits))
        {
            var attributes = type
                .GetCustomAttributes(typeof(TAttribute), inherits);

            if (attributes.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Entity type has more than one instance of the attribute {typeof(TAttribute).FullName}");
            }

            return (TAttribute)attributes[0];
        }

        return null;
    }


    public static IEnumerable<Type> FindCloseInterfacesOf(this Type source, Type openGeneric)
    {
        return source
            .FindCloseInterfacesOfCore(openGeneric)
            .Distinct();
    }

    // public static IEnumerable<Type> FindImplementedType<T>(this Type source, bool concreteOnly)
    // {
    //     return FindImplementedType(source, typeof(T), concreteOnly);
    // }

    // public static IEnumerable<Type> FindImplementedType(this Type source, Type lookUp, bool concreteOnly)
    // {
    //     if (source.BaseType == typeof(object))
    //     {
    //         yield break;
    //     }
    //
    //     if (source.BaseType.IsConcrete() || concreteOnly && source.BaseType == lookUp)
    //     {
    //         yield return source.BaseType;
    //     }
    //
    //     foreach (var result in source.BaseType.FindImplementedType(lookUp, concreteOnly))
    //     {
    //         yield return result;
    //     }
    // }

    /// <param name="source">Must be a concrete type</param>
    /// <returns>Returns all the types the source implements/inherits that are closed generics of the open one passed as the second argument and that have no generic parameters (closed generics)</returns>
    private static IEnumerable<Type> FindCloseInterfacesOfCore(this Type source, Type openGeneric)
    {
        if (openGeneric == null)
        {
            yield break;
        }

        if (!source.IsConcrete())
        {
            yield break;
        }

        if (openGeneric.IsInterface)
        {
            foreach (var sourceInterfaceType in source
                         .GetInterfaces()
                         .Where(t => t.ClosedGenericOf(openGeneric)))
            {
                yield return sourceInterfaceType;
            }
        }
        else if (source.BaseType!.ClosedGenericOf(openGeneric))
        {
            yield return source.BaseType;
        }

        if (source.BaseType == typeof(object))
        {
            yield break;
        }

        foreach (var interfaceType in FindCloseInterfacesOf(source.BaseType, openGeneric))
        {
            yield return interfaceType;
        }
    }


    public static bool ClosedGenericOf(this Type source, Type openGeneric)
    {
        return source.IsGenericType && !source.ContainsGenericParameters &&
               source.GetGenericTypeDefinition() == openGeneric;
    }

    public static bool IsConcrete(this Type type)
    {
        return !type.IsAbstract && !type.IsInterface;
    }

    public static IEnumerable<Type> GetTypesAssignableFrom(this Type source)
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