using EShop.Core.Catalog.Products.Price;
using EShop.Core.Checkout.Orders.Domain;
using EShop.Core.Data.Cart.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext 
{
    public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
    public DbSet<DiscountUsageHistory> DiscountUsageHistories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}