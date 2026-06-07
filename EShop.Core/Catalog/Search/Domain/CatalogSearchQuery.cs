using EShop.Core.Platform.Search;

namespace EShop.Core.Data.Search.Domain;

public class CatalogSearchQuery : SearchQuery<CatalogSearchQuery>
{
    public CatalogSearchQuery(string[] fields, string term, SearchMode model = SearchMode.Contains) : base(fields, term, model)
    {
    }
}