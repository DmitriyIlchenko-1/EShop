using EShop.Core.Catalog.Configuration;
using EShop.Core.Data;
using EShop.Infrastructure.Http;
using Microsoft.AspNetCore.Http;

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
            string.Join(',', newProductIds
                .Take(_catalogSettings.RecentlyViewedProductsNumber)),
            options);
    }

    private IEnumerable<int> GetRecentlyViewedProductIds(int count)
    {
        var request = _httpContextAccessor?.HttpContext?.Request;
        if (request != null && request.Cookies.TryGetValue(CookieNames.RecentlyViewedProducts, out string value))
        {
            var ids = value
                .Split(',')
                .Select(x => Convert.ToInt32(x))
                .Where(id => id > 0)
                .Distinct()
                .Take(count);

            return ids;
        }

        return [];
    }
}