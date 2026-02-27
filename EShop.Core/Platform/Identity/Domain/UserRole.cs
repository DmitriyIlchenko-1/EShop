using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Platform.Identity.Domain;

internal class UserRoleMap : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
        
    }
}

public class UserRole : IdentityUserRole<int>
{
    public User User { get; set; }
    public Role Role { get; set; }
}