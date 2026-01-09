using Microsoft.Extensions.ObjectPool;

namespace EShop.Infrastructure.Utilities;

public static class ObjectPoolExtensions
{
    public static IDisposable Get<T>(this ObjectPool<T> pool, out T result) where T : class
    {
        var pooledValue = pool.Get();
        result = pooledValue;
        return new ActionDisposable(() => pool.Return(pooledValue));
    }
}