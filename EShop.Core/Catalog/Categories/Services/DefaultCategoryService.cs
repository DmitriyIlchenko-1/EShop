using EShop.Core.Catalog.Categories.Domain;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data.Categories.Services;

public class DefaultCategoryService : ICategoryService
{
    private readonly ApplicationDbContext _db;

    public DefaultCategoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ICollection<ProductCategory>> GetAllCategoriesByProductIds(int[] productIds,
        bool tracking = false,
        bool includeHidden = false)
    {
        Guard.NotNull(productIds);
        if (!productIds.Any())
        {
            return [];
        }

        var categoryQuery = _db
            .Categories.OrderBy(x => x.ParentId)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .AsQueryable();
        
        if (!includeHidden)
            categoryQuery = categoryQuery.Where(x => x.IsPublished);
        
        var productCategoryQuery = _db.ProductCategories
            .Include(x => x.Category);
        
        var query = from pc in productCategoryQuery
            join c in categoryQuery on pc.ProductId equals c.Id
            where productIds.Contains(pc.ProductId)
            orderby pc.DisplayOrder
            select pc;
        
        return await query.ApplyTracking(tracking).ToListAsync();
        
    }
}