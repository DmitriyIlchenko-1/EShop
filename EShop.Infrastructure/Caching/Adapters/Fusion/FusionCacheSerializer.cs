// using EShop.Infrastructure.Utilities;
// using ZiggyCreatures.Caching.Fusion.Serialization;
//
// namespace EShop.Infrastructure.Caching.Adapters.Fusion;
//
// public class FusionCacheSerializer : IFusionCacheSerializer
// {
//     private readonly IJsonSerializer _jsonSerializer;
//
//     public FusionCacheSerializer(IJsonSerializer jsonSerializer)
//     {
//         _jsonSerializer = jsonSerializer;
//     }
//
//     public byte[] Serialize<T>(T obj)
//     {
//         _jsonSerializer.TrySerialize(obj, out var buffer);
//         return buffer;
//     }
//
//     public T Deserialize<T>(byte[] data)
//     {
//         _jsonSerializer.TryDeserialize<T>(data, out T obj);
//         return obj;
//     }
//
//     public ValueTask<byte[]> SerializeAsync<T>(T obj, CancellationToken token = new CancellationToken())
//     {
//         return new ValueTask<byte[]>(Serialize<T>(obj));
//     }
//
//     public ValueTask<T> DeserializeAsync<T>(byte[] data, CancellationToken token = new CancellationToken())
//     {
//         return new ValueTask<T>(Deserialize<T>(data));
//     }
// }