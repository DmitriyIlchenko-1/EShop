namespace EShop.Infrastructure.Extensions;

public static class EnumerableExtensions
{
    public static void Each<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source is List<T> list)
        {
            list.ForEach(action);
        }
        else
        {
            foreach (T item in source)
            {
                action(item);
            }
        }

         
    }


    public static IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source,
        Func<TSource, ValueTask<TResult>> predicate)
    {
        return source
            .ToAsyncEnumerable()
            .SelectAwait(predicate);
    }
}