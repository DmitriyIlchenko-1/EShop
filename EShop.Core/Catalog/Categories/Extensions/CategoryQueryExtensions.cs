using EShop.Core.Catalog.Categories.Domain;

namespace EShop.Core.Catalog.Categories.Extensions;

public static class CategoryQueryExtensions
{
    public static IQueryable<Category> ApplyStandardFilters(this IQueryable<Category> query, bool onlyPublished = true)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (onlyPublished)
            query = query.Where(x => x.IsPublished);

        return query
            .OrderBy(x => x.ParentId)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name);

    }
}