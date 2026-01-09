using EShop.Core.Catalog.Brands.Domain;

namespace EShop.Core.Catalog.Brands.Extensions;

public static class BrandQueryExtensions
{
    public static IQueryable<Brand> ApplyStandardFilters(this IQueryable<Brand> query, bool publishedOnly = true)
    {
        if (publishedOnly)
            query = query.Where(x => x.IsPublished);

        return query
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name);
    }
}