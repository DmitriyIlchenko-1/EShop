using EShop.Core.Platform.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<ExternalIdentityLogin> ExternalIdentityLogins { get; set; }
}