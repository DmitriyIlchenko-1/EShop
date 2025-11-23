using EShop.Core.Platform.Configuration.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext 
{
    public DbSet<Setting> Settings { get; set; }
}