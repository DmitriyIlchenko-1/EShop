using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Common.Domain;
using EShop.Core.Platform.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext
{
    public DbSet<Address> Addresses { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Label> Labels { get; set; }
    public DbSet<DeliveryTime> DeliveryTimes { get; set; }
     public DbSet<UserAddress> UserAddresses { get; set; }
   
    
}