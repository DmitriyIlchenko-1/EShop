using System.Collections.ObjectModel;
using EShop.Infrastructure.Utilities;

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

    public static ReadOnlySet<T> AsReadOnly<T>(this ISet<T> set)
    {
        if (set.Count == 0)
        {
            return ReadOnlySet<T>.Empty;
        }

        return new ReadOnlySet<T>(set);
    }

    public static ReadOnlyCollection<T> AsReadOnly<T>(this IEnumerable<T> source)
    {
        if (source is null)
        {
            return ReadOnlyCollection<T>.Empty;
        }
        else if (source is ReadOnlyCollection<T> typed)
        {
            return typed;
        }

        if (source.TryGetNonEnumeratedCount(out var count) && count == 0)
        {
            return ReadOnlyCollection<T>.Empty;
        }

        if (!source.Any())
        {
            return ReadOnlyCollection<T>.Empty;
        }

        if (source is List<T> list)
        {
            return list.AsReadOnly();
        }
        else if (source is IList<T> iList)
        {
            return new ReadOnlyCollection<T>(iList);
        }
        
        return new ReadOnlyCollection<T>(source.ToList());
    }


    public static IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source,
        Func<TSource, ValueTask<TResult>> predicate)
    {
        return source
            .ToAsyncEnumerable()
            .SelectAwait(predicate);
    }
}