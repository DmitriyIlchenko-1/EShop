using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Checkout.Order.Domain;

public class Order : BaseEntity
{
    public User User { get; set; }
    public int UserId { get; set; }
}