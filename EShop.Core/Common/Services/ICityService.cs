using System.Globalization;
using EShop.Core.Common.Domain;
using EShop.Core.Data;
using EShop.Core.Platform.Caching;
using EShop.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Common.Services;

public interface ICityService
{
    Task<ICollection<City>> GetAllAsync(bool showHidden = false);
}


public class CityService : ICityService
{
    private readonly ApplicationDbContext _db;
    private const string CitiesAllCacheKey = "cities:all:{0}"; //showHidden
    private readonly   ICacheManager _cacheManager;

    public CityService(ApplicationDbContext db, ICacheManager cacheManager)
    {
        _db = db;
        _cacheManager = cacheManager;
    }

    public async Task<ICollection<City>> GetAllAsync(bool showHidden = false)
    {
        var cacheKey = string.Format(CultureInfo.InvariantCulture, CitiesAllCacheKey, showHidden);
        return await _cacheManager.GetOrCreateAsync(cacheKey,
            async () =>
            {
                var query = _db.Cities.AsQueryable();
                if (!showHidden)
                {
                    query.Where(x => x.IsCityEnabled);
                }

                return await query
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Name)
                    .ToListAsync();
            }, new CacheEntryOptions(TimeSpan.FromHours(999)));
    }
}