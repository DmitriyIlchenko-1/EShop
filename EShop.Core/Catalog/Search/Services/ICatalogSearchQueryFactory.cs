using EShop.Core.Data.Search.Domain;

namespace EShop.Core.Data.Search.Services;

public interface ICatalogSearchQueryFactory
{
    public CatalogSearchQuery Current { get; }
    public CatalogSearchQuery CreateFromQuery();
}