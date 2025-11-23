using EShop.Core.Platform.Identity.Domain;

namespace EShop.Core.Platform.Identity.Extensions;

public static class UserExtensions
{
    public static bool IsRegistered(this User user, bool onlyActiveRoles = true)
    {
        return user.IsInRole(UserRoleNameConstants.Registered, onlyActiveRoles);
    }

    public static bool IsGuest(this User user, bool onlyActiveRoles = true)
    {
        bool isGuest = false;
        bool isRegistered = false;

        foreach (var roleName in user.GetRoleNames(onlyActiveRoles))
        {
            if (!isGuest && string.Equals(roleName, UserRoleNameConstants.Guest))
            {
                isGuest = true;
            }

            if (!isRegistered && string.Equals(roleName, UserRoleNameConstants.Registered))
            {
                isRegistered = true;
            }

            if (isGuest && isRegistered)
            {
                break;
            }
        }

        return isGuest && !isRegistered;
    }

    public static IEnumerable<string> GetRoleNames(this User user, bool onlyActiveRoles = true)
    {
        foreach (var r in user.UserRoles)
        {
            var role = r.Role;
            if ((!onlyActiveRoles || role.Active))
            {
                yield return role.Name;
            }
        }
    }

    public static bool IsInRole(this User user, string roleName, bool onlyActiveRoles = true)
    {
        foreach (var userRole in user.UserRoles)
        {
            var role = userRole.Role;
            if ((!onlyActiveRoles || role.Active) && role.Name!.Equals(roleName))
            {
                return true;
            }
        }

        return false;
    }
}