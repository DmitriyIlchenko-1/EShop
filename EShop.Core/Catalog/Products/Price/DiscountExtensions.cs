using EShop.Infrastructure.Utilities;

namespace EShop.Core.Catalog.Products.Price;

public static class DiscountExtensions
{
    public static decimal GetDiscountAmount(this Discount discount, decimal price)
    {
        Guard.NotNull(discount);
        return discount.UsePercentage
            ? price * discount.DiscountAmount / 100m
            : discount.DiscountAmount;
    }

    public static Discount GetBestCandidateDiscount(this ICollection<Discount> discounts, decimal price)
    {
        Guard.NotNull(discounts);
        decimal highestAmount = 0;
        Discount bestCandidate = null;
        foreach (var discount in discounts)
        {
            var discountAmount = discount.GetDiscountAmount(price);
            if (highestAmount < discountAmount)
            {
                highestAmount = discountAmount;
                bestCandidate = discount;
            }
        }

        return bestCandidate;
    }

    public static bool IsValidDateTime(this Discount discount)
    {
        Guard.NotNull(discount);
        var now = DateTime.UtcNow;
        if (discount.StartsOnUtc.HasValue && discount.StartsOnUtc.Value.CompareTo(now) > 0)
        {
            return false;
        }

        if (discount.EndsOnUtc.HasValue && discount.EndsOnUtc.Value.CompareTo(now) <= 0)
        {
            return false;
        }

        return true;
    }
}