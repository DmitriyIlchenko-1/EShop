using System.Globalization;
using EShop.Core.Common.Domain;
using EShop.Core.Data;
using EShop.Core.Platform.Caching;
using EShop.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Common.Services;

public interface ICityService
{
    Task<ICollection<City>> GetAllAsync();
    Task<City> GetByIdAsync(int cityId);
}

public class CityService : ICityService
{
    private readonly ApplicationDbContext _db;
    private const string CitiesAllCacheKey = "cities:all"; 
    private readonly ICacheManager _cacheManager;

    public CityService(ApplicationDbContext db, ICacheManager cacheManager)
    {
        _db = db;
        _cacheManager = cacheManager;
    }

    public async Task<ICollection<City>> GetAllAsync()
    {
        return await PrefetchCitiesAsync();
    }

    public async Task<City> GetByIdAsync(int cityId)
    {
        if (cityId < 1)
        {
            return null;
        }
        var cities = await PrefetchCitiesAsync();
        return cities.FirstOrDefault(x => x.Id == cityId);
    }

    protected virtual async Task<City[]> PrefetchCitiesAsync()
    {
        var cacheKey = string.Format(CultureInfo.InvariantCulture, CitiesAllCacheKey);
        return await _cacheManager.GetOrCreateAsync(cacheKey,
            async () =>
            {
                var query = _db.Cities.AsQueryable();
                return await query
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Name)
                    .ToArrayAsync();
            });
    }
}