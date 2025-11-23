using EShop.Core.Platform.Logging.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext
{
    public DbSet<ActivityLogType> ActivityLogTypes { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
}