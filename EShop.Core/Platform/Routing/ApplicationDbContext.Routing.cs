using EShop.Core.Platform.Routing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext
{
    public DbSet<EntityType> EntityTypes { get; set; }
    public DbSet<UrlRecord> UrlRecords { get; set; }
    
}