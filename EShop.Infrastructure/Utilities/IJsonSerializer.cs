using System.Diagnostics.CodeAnalysis;

namespace EShop.Infrastructure.Utilities;

public interface IJsonSerializer
{
    bool CanSerialize(object item);
    bool CanDeserialize(object item);
    bool CanDeserialize(Type objType);
    bool TrySerialize(object value, [MaybeNullWhen(false)] out byte[] result);
    bool TryDeserialize<T>(byte[] value, [MaybeNullWhen(false)] out object result);
    bool TryDeserialize<T>(byte[] value, [MaybeNullWhen(false)] out T result);
    
}