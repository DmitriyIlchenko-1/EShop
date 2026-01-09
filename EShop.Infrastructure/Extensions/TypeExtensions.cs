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
}