using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace EShop.Core.Platform.Identity.Extensions;

public static class UserQueryExtensions
{
    public static IIncludableQueryable<User, Role> IncludeRoles(this IQueryable<User> query)
    {
        Guard.NotNull(query);
        return query.Include(x => x.UserRoles)
            .ThenInclude(x => x.Role);
    }
}