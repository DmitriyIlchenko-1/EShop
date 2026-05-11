using EShop.Core.Catalog.Products.Price;
using EShop.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Catalog.Products.Services;

public interface IDiscountService
{
    Task<ICollection<Discount>> GetAppliedDiscountsByProductIdAsync(int productId, bool withCoupon = true);
}

public class DefaultDiscountService : IDiscountService
{
    private readonly ApplicationDbContext _db;

    public DefaultDiscountService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ICollection<Discount>> GetAppliedDiscountsByProductIdAsync(int productId, bool withCoupon = true)
    {
        if (productId == 0)
            return [];
        
        return await _db.Discounts.AsNoTracking()
            .Where(x => x.AppliedToProducts.Any(y => y.Id == productId))
            .Where(x => x.EndsOnUtc > DateTime.UtcNow && x.StartsOnUtc <= DateTime.UtcNow)
            .Where(x => x.IsCouponRequired == withCoupon)
            .ToListAsync();
    }
}