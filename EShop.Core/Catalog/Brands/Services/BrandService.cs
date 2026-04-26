using EShop.Core.Catalog.Brands.Domain;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data.Brands.Services;

public class DefaultBrandService : IBrandService
{
    private readonly ApplicationDbContext _db;

    public DefaultBrandService(ApplicationDbContext db)
    {
        _db = db;
    }

    public virtual async Task<ICollection<Brand>> GetBrandsByIdsAsync(int[] brandIds,
        bool includeUnpublished = false, bool track = false)
    {
        Guard.NotNull(brandIds);
        if (brandIds.Length == 0)
        {
            return [];
        }

        var query = _db.Brands.AsQueryable();
        if(track)
            query = query.AsNoTracking();
        return await query
            .Where(x => includeUnpublished || x.IsPublished)
            .Where(x => brandIds.Contains(x.Id))
            .ToListAsync();
    }
}