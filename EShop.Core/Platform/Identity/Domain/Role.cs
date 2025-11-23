using EShop.Infrastructure.Domain;
using Microsoft.AspNetCore.Identity;

namespace EShop.Core.Platform.Identity.Domain;

public class Role : IdentityRole<int>, IEntityWithTypedId<int>
{
    public bool Active { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
}