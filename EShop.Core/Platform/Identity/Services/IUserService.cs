using EShop.Core.Platform.Identity.Domain;

namespace EShop.Core.Platform.Identity.Services;

public interface IUserService
{
    Task<User?> GetAuthenticatedUserAsync();
    Task<User?> GetUserByIdentityAsync(string visitorIdentity, int maxAge = 60);
    Task<User> CreateVisitorUserAsync(string clientIdentity, Action<User>? configure = null);
    Task<Role?> GetUserRoleByNameAsync(string roleName, bool tracking = true);
    void AppendVisitorCookie(User user);
}