using EShop.Core.Catalog.Configuration;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Catalog.Products.Services;

public class RecentlyViewedProductsService : IRecentlyViewedProductsService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CatalogSettings _catalogSettings;

    public RecentlyViewedProductsService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor,
        CatalogSettings catalogSettings)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _catalogSettings = catalogSettings;
    }

    public void AddProductToRecentlyViewedList(int productId)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            return;
        }

        var existingProductIds = GetRecentlyViewedProductIds(int.MaxValue);
        var newProductIds = new List<int>(existingProductIds);
        newProductIds.Remove(productId);
        newProductIds.Insert(0, productId);
        var cookies = _httpContextAccessor.HttpContext.Response.Cookies;
        var options = new CookieOptions()
        {
            Expires = DateTime.Now.AddDays(10),
            HttpOnly = true,
            IsEssential = true
        };

        cookies.Append(CookieNames.RecentlyViewedProducts,
            string.Join(',',
                newProductIds
                    .Take(_catalogSettings.RecentlyViewedProductsNumber)),
            options);
    }

    public async Task<IList<Product>> GetRecentlyViewedProducts(int count, int? productToSkipId = null)
    {
        var productIds = GetRecentlyViewedProductIds(count, productToSkipId);
        if (!productIds.Any())
        {
            return [];
        }

        var products = await _db
            .Products.AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .Where(x => x.Published)
            .SelectSummaryOnly()
            .OrderBy(x => x.Id)
            .Take(count)
            .ToListAsync();
        
        //TODO: order property.
        return products;


    }

    private IEnumerable<int> GetRecentlyViewedProductIds(int count, int? productToSkipId = null)
    {
        var request = _httpContextAccessor?.HttpContext?.Request;
        if (request != null && request.Cookies.TryGetValue(CookieNames.RecentlyViewedProducts, out string value))
        {
            var ids = value
                .Split(',')
                .Select(x => Convert.ToInt32(x))
                .Where(id => id > 0);

            if (productToSkipId.HasValue)
            {
                ids = ids.Where(x => x != productToSkipId.Value);
            }

            return ids
                .Distinct()
                .Take(count);
        }

        return [];
    }
}