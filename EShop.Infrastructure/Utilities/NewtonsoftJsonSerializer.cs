using System.Text;
using EShop.Infrastructure.Extensions;
using Newtonsoft.Json;

namespace EShop.Infrastructure.Utilities;

public class NewtonsoftJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializer _serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings()
        { TypeNameHandling = TypeNameHandling.None });

    private readonly HashSet<Type> _unserializableTypes = [typeof(Task), typeof(Task<>)];
    private readonly HashSet<Type> _undeserializableTypes = [typeof(Task), typeof(Task<>)];

    public bool CanSerialize(object item)
        => IsSupportedTypeInternal(item?.GetType(), _unserializableTypes);

    public bool CanDeserialize(object item)
        => IsSupportedTypeInternal(item?.GetType(), _undeserializableTypes);

    public bool CanDeserialize(Type objType)
        => IsSupportedTypeInternal(objType, _undeserializableTypes);

    public bool TrySerialize(object value, out byte[] result)
    {
        result = null;

        if (!CanSerialize(value))
        {
            return false;
        }

        try
        {
            result = SerializeInternal(value);
            return true;
        }
        catch
        {
            if (value != null)
            {
                _unserializableTypes.Add(value.GetType());
            }

            return false;
        }
    }

    public bool TryDeserialize<T>(byte[] value, out T item)
    {
        item = default(T);
        var result = TryDeserialize<T>(value, out object obj);
        if (result)
        {
            item = (T)obj;
        }

        return result;
    }
    
    public bool TryDeserialize<T>(byte[] value, out object result)
    {
        var objType = typeof(T);
        result = null;
        if (!CanDeserialize(objType))
        {
            return false;
        }

        try
        {
            result = DeserializeInternal(objType, value);
            return true;
        }
        catch
        {
            if (!(objType == typeof(object) || objType.IsBasicOrNullableType()))
            {
                _undeserializableTypes.Add(objType);
            }

            return false;
        }
    }


    private byte[] SerializeInternal(object item)
    {
        if (item == null)
        {
            return null;
        }

        using var d = StringBuilderPool.Pool.Get(out var stringBuilder);
        using var writer = new StringWriter(stringBuilder);
        _serializer.Serialize(writer, item);
        var serializedResult = Encoding.UTF8.GetBytes(stringBuilder.ToString());
        return serializedResult;
    }

    private object DeserializeInternal(Type objType, byte[] item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        var json = Encoding.UTF8.GetString(item);
        using var reader = new StringReader(json);
        return _serializer.Deserialize(reader, objType);
    }


    private bool IsSupportedTypeInternal(Type objType, HashSet<Type> unsupportedTypes)
    {
        if (objType == null)
        {
            return true;
        }

        if (unsupportedTypes.Contains(objType))
        {
            return false;
        }

        if (objType.IsGenericType && unsupportedTypes.Contains(objType.GetGenericTypeDefinition()))
        {
            return false;
        }

        return true;
    }
}