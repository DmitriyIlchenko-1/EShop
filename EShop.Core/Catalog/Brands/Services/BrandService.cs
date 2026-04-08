using EShop.Core.Catalog.Brands.Domain;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data.Brands.Services;

 
public class BrandService : IBrandService
{
    private readonly ApplicationDbContext _db;

    public BrandService(ApplicationDbContext db)
    {
        _db = db;
    }

    public virtual async Task<ICollection<ProductBrand>> GetBrandsByProductIdsAsync(int[] productIds, bool includeUnpublished = false)
    {
        Guard.NotNull(productIds);
        if (productIds.Length == 0)
        {
            return [];
        }

        var brandQuery = _db
            .Brands.AsNoTracking()
            .Where(x => x.IsPublished == includeUnpublished);
        
        var productBrandQuery = _db
            .ProductBrands.AsNoTracking()
            .Include(x => x.Brand)
            .Where(x => productIds.Contains(x.ProductId));
        
        var query = from pb in productBrandQuery
            join b in brandQuery on pb.BrandId equals b.Id
            orderby pb.DisplayOrder
            select pb;
        return await query.ToListAsync();
    }
}