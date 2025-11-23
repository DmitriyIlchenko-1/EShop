using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EShop.Core.Platform.Identity.Services;

public class CustomRoleStore : RoleStore<Role, ApplicationDbContext, int, UserRole, IdentityRoleClaim<int>>
{
    public CustomRoleStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null) : base(context,
        describer)
    {
    }
    
    
    
}