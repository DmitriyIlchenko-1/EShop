using EShop.Core.Catalog.Products.Price;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext 
{
    public DbSet<DiscountUsageHistory> DiscountUsageHistories { get; set; }
}