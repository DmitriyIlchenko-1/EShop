using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Collections;

public class KeysetPaginator<T> where T : BaseEntity
{
    private int _lastId;
    private bool _hasNext;
    private readonly int _pageSize;
    private readonly IQueryable<T> _source;
    private T[] _lastResult;

    public KeysetPaginator(IQueryable<T> source, int pageSize)
    {
        _source = source.OrderBy(x => x.Id);
        _pageSize = pageSize;
    }

    public async Task<bool> ReadNextPageAsync()
    {
        if (_hasNext)
            return false;
        var nextPage = await _source
            .Where(x => x.Id > _lastId)
            .Take(_pageSize)
            .ToArrayAsync();
        _lastResult = nextPage;
        _hasNext = _lastResult.Length != 0;
        if (_hasNext)
        {
            _lastId = nextPage.Last()
                .Id;
        }

        return _hasNext;
    }

    public T[] GetResult()
    {
        return _lastResult;
    }
}

public struct OutAsync<T>
{
    private readonly bool _success;
    private readonly T _source;

    public OutAsync(bool success, T source = default(T))
    {
        _success = success;
        _source = source;
    }

    public bool TryGetNext(out T result)
    {
        result = _source;
        return _success;
    }
}