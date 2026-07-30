using EShop.Core.Checkout.Orders.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data.Orders.Extensions;

public static class OrderQueryExtensions
{
    public static IQueryable<Order> ApplyStandardFilter
        (this IQueryable<Order> query, int? userId = null, bool includeOrderItems = true, bool loadDeleted = false )
    {
        if (!loadDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        if (userId.HasValue && userId.Value > 0)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (includeOrderItems)
        {
            query = query.Include(x => x.OrderItems).ThenInclude(x => x.Product);
        }

        return query.Include(x => x.ShippingAddress);
    }
}