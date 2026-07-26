using System.Globalization;
using System.Text;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Checkout.Orders.Domain;
using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Extensions;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Media.Images;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Catalog.Products.Services;

public interface IDiscountService
{
    Task<ICollection<Discount>> GetAppliedDiscountsByProductIdAsync(int productId, bool withCoupon = true);

    Task<ICollection<Discount>> GetAllDiscountsAsync(DiscountType? discountType, string couponCode = null,
        bool tracking = false,
        bool includeHidden = false);

    Task<bool> IsDiscountValidAsync(Discount discount, User user);
}

public class DefaultDiscountService : IDiscountService
{
    // {0} - type, {1} - coupon code,  {2} - hidden?.
    private readonly static CompositeFormat DiscountsAllKeyFormat = CompositeFormat.Parse("discount.all-{0}-{1}-{2}");
    private readonly ApplicationDbContext _db;
    private readonly IRequestCache _requestCache;

    private readonly Dictionary<DiscountCacheKey, bool> _discountValidityCache = [];

    public DefaultDiscountService(ApplicationDbContext db, IRequestCache requestCache)
    {
        _db = db;
        _requestCache = requestCache;
    }

    public async Task<ICollection<Discount>> GetAllDiscountsAsync(DiscountType? discountType, string couponCode = null,
        bool tracking = false,
        bool includeHidden = false)
    {
        couponCode ??= string.Empty;
        var cacheKey = string.Format(CultureInfo.InvariantCulture,
            DiscountsAllKeyFormat,
            discountType,
            couponCode,
            includeHidden);
        return await _requestCache.GetOrCreateAsync(cacheKey,
            async () =>
            {
                var query = _db.Discounts.ApplyTracking(tracking);
                if (discountType.HasValue)
                {
                    switch (discountType.Value)
                    {
                        case DiscountType.ProductDiscount:
                            query = query.Include(x => x.AppliedToProducts);
                            break;
                        case DiscountType.CategoryDiscount:
                            query = query.Include(x => x.AppliedToCategories);
                            break;
                    }

                    query = query.Where(x => x.DiscountType == discountType.Value);
                }

                if (includeHidden)
                {
                    var utcNow = DateTime.UtcNow;
                    query = query.Where(x => (!x.StartsOnUtc.HasValue || x.StartsOnUtc.Value <= utcNow)
                                             && (!x.EndsOnUtc.HasValue || x.EndsOnUtc.Value > utcNow));
                }

                if (!couponCode.IsEmpty())
                {
                    query = query.Where(x => x.CouponCode == couponCode);
                }

                return await query
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            });
    }

    public async Task<ICollection<Discount>> GetAppliedDiscountsByProductIdAsync(int productId, bool withCoupon = true)
    {
        if (productId == 0)
            return [];

        return await _db
            .Discounts.AsNoTracking()
            .Where(x => x.AppliedToProducts.Any(y => y.Id == productId))
            .Where(x => x.EndsOnUtc > DateTime.UtcNow && x.StartsOnUtc <= DateTime.UtcNow)
            .Where(x => x.IsCouponRequired == withCoupon)
            .ToListAsync();
    }

    public virtual async Task<bool> IsDiscountValidAsync(Discount discount, User user)
    {
        Guard.NotNull(discount);
        var cacheKey = new DiscountCacheKey(discount, user, user.DiscountCouponCode);
        if (_discountValidityCache.TryGetValue(cacheKey, out var isValid))
        {
            return isValid;
        }

        if (discount.IsCouponRequired && (discount.CouponCode.IsEmpty() || !discount
                .CouponCode.Trim()
                .Equals(user.DiscountCouponCode, StringComparison.OrdinalIgnoreCase)))
        {
            Cache(false);
        }

        if (!discount.IsValidDateTime())
        {
            return Cache(false);
        }

        if (!await CheckDiscountLimitations(discount, user))
        {
            Cache(false);
        }

        return Cache(true);

        bool Cache(bool res) => _discountValidityCache[cacheKey] = res;
    }

    protected virtual async Task<bool> CheckDiscountLimitations(Discount discount, User user)
    {
        switch (discount.CouponUsageType)
        {
            case CouponUsageType.NTimesOnly:
            {
                var count = await _db
                    .DiscountUsageHistories.AsNoTracking()
                    .CountAsync(x => x.DiscountId == discount.Id);
                return count < discount.AppliedTimes;
            }
            case CouponUsageType.NTimesPerUser:
            {
                if (user != null && !user.IsGuest())
                {
                    var count = await _db
                        .DiscountUsageHistories.AsNoTracking()
                        .Include(x => x.Order)
                        .CountAsync(x => x.DiscountId == discount.Id
                                         && x.Order.UserId == user.Id);
                    return count < discount.AppliedTimes;
                }

                return true;
            }
            case CouponUsageType.Unlimited:
                return true;
            default:
                return false;
        }
    }
}

public record DiscountCacheKey(Discount Discount, User User, string CouponCode);