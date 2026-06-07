using EShop.Core.Data.Search.Domain;
using EShop.Core.Platform.Configuration.Domain;
using EShop.Core.Platform.Search;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;

namespace EShop.Core.Data.Search.Services;

public class DefaultCatalogSearchQueryFactory : ICatalogSearchQueryFactory
{
    private readonly HttpContext _httpContext;
    protected readonly SearchSettings _searchSettings;
    

    public DefaultCatalogSearchQueryFactory(IHttpContextAccessor httpContextAccessor, SearchSettings searchSettings)
    {
        _searchSettings = searchSettings;
        _httpContext = httpContextAccessor?.HttpContext;
    }

    protected virtual string[] Tokens => ["q"];

    public CatalogSearchQuery Current { get; private set; }
    public CatalogSearchQuery CreateFromQuery()
    {
        if (_httpContext == null)
        {
            return null;
        }

        var fields = _searchSettings.SearchFields;
        var searchTerm = _httpContext.Request.Query["q"].ToString();
        var query = new CatalogSearchQuery(fields.ToArray(), searchTerm);
        Current = query;
        return query;

    }
    
    
    

   
}

public class SearchSettings : ISettings
{
    public List<string> SearchFields { get; set; } = ["name", "shortdescription", "keyword", "brand", "category"];
    public SearchMode SearchMode { get; set; } = SearchMode.Contains;
    public int InstantSearchMaxResultNumber { get; set; }
}