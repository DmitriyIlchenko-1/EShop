using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EShop.Core.Platform.Identity.Services;

public class CustomUserStore : UserStore<User, Role, ApplicationDbContext, int, IdentityUserClaim<int>, UserRole,
    IdentityUserLogin<int>, IdentityUserToken<int>, IdentityRoleClaim<int>>
{
    public CustomUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null) : base(context,
        describer)
    {
    }


    public override Task SetEmailConfirmedAsync(User user, bool confirmed,
        CancellationToken cancellationToken = new CancellationToken())
    {
        ArgumentNullException.ThrowIfNull(user);
        user.Active = confirmed;
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }
}