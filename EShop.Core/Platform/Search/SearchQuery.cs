using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Platform.Search;

public interface ISearchQuery
{
    public string DefaultTerm { get; set; }
    public int Skip { get; }
    public int Take { get; }
}


public class SearchQuery<TQuery> : ISearchQuery where TQuery : class, ISearchQuery
{
    public string DefaultTerm { get; set; }
    public int Skip { get; protected set; }
    public int Take { get; protected set; } = Int32.MaxValue;

    public TQuery Slice(int skip, int take)
    {
        Guard.NotNegative(skip);
        Guard.NotNegative(take);
        Skip = skip;
        Take = take;
        return (this as TQuery);
    }

    protected SearchQuery(string[]? fields, string? term, SearchMode model = SearchMode.Contains)
    {
        if (term.HasValue())
        {
            if (fields == null || !fields.Any(x => x.HasValue()))
            {
                throw new ArgumentException("At least one field value must be present to search by term", nameof(fields));
            }

            if (fields.Length == 1)
            {
                
            }
        }
    }
}


public enum SearchMode
{
    Contains,
    StartsWith,
    ExactMatch
}