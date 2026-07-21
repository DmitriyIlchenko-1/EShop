namespace EShop.Infrastructure.Caching;

public interface IRequestCache
{
    T GetOrCreate<T>(object key, Func<T> factory);
    Task<T> GetOrCreateAsync<T>(object key, Func<Task<T>> factory);
    void Put(object key, object value);
    T Get<T>(object key);
    bool TryGet<T>(object key, out T result);
    bool Contains(object key);
    void Remove(object key);
    
}