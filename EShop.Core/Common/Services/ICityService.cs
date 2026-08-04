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
    Task<City> GetByIdAsync(int cityId, bool showHidden = false);
}

public class CityService : ICityService
{
    private readonly ApplicationDbContext _db;
    private const string CitiesAllCacheKey = "cities:all:{0}"; //showHidden
    private readonly ICacheManager _cacheManager;

    public CityService(ApplicationDbContext db, ICacheManager cacheManager)
    {
        _db = db;
        _cacheManager = cacheManager;
    }

    public async Task<ICollection<City>> GetAllAsync(bool showHidden = false)
    {
        return await PrefetchCitiesAsync(showHidden);
    }

    public async Task<City> GetByIdAsync(int cityId, bool showHidden = false)
    {
        if (cityId < 1)
        {
            return null;
        }
        var cities = await PrefetchCitiesAsync(showHidden);
        return cities.FirstOrDefault(x => x.Id == cityId);
    }

    protected virtual async Task<City[]> PrefetchCitiesAsync(bool showHidden = false)
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
                    .ToArrayAsync();
            },
            new CacheEntryOptions(TimeSpan.FromHours(999)));
    }
}