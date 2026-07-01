using System.Security.Claims;
using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Web;
using EShop.Infrastructure.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Platform.Identity.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;
    private readonly IWebHelper _webHelper;
    private bool _authenticatedUserResolved;
    private User? _authenticatedUser;

    public UserService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext db, IWebHelper webHelper, UserManager<User> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _db = db;
        _webHelper = webHelper;
        _userManager = userManager;
    }

    public async Task<User?> GetAuthenticatedUserAsync()
    {
        if (!_authenticatedUserResolved)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            var principal = await EnsureAuthenticated(httpContext);

            if (principal?.Identity?.IsAuthenticated == true)
            {
                _authenticatedUser = await _userManager.GetUserAsync(principal);
            }

            _authenticatedUserResolved = true;
        }

        if (_authenticatedUser == null || !_authenticatedUser.IsActive || _authenticatedUser.IsDeleted)
        {
            return null;
        }

        return _authenticatedUser;
    }

    public async Task<User> CreateVisitorUserAsync(string clientIdentity, Action<User>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(clientIdentity))
        {
            throw new ArgumentException("Client identity cannot be null or empty.", nameof(clientIdentity));
        }

        User user = new User()
        {
            UserGuid = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow,
            LastActivityDateUtc = DateTime.UtcNow,
            ClientIdentity = clientIdentity,
            IsActive = true,
        };

        var visitorRole = await GetUserRoleByNameAsync(UserRoleNameConstants.Guest, true) ??
                          throw new InvalidOperationException("No guest role found.");

        user.UserRoles.Add(new UserRole()
        {
            UserId = user.Id,
            Role = visitorRole
        });

        configure?.Invoke(user);
        //TODO: limit db handlers to only essential ones.
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public Task<Role?> GetUserRoleByNameAsync(string roleName, bool tracking = true)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return Task.FromResult<Role>(null);
        }

        var query = tracking ? _db.Roles.AsTracking() : _db.Roles.AsNoTracking();
        return query.FirstOrDefaultAsync(x => x.Name == roleName);
    }

    public void AppendVisitorCookie(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && user.UserGuid != Guid.Empty)
        {
            //TODO: create and inject privacy settings and take the value from them. 
            var cookieExpires = DateTime.Now.AddDays(2);

            var cookieOptions = new CookieOptions
            {
                Expires = cookieExpires,
                IsEssential = true,
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            };

            httpContext.Response.Cookies.Append(CookieNames.Visitor, user.UserGuid.ToString(), cookieOptions);
        }
    }

    public async Task<User?> GetUserByIdentityAsync(string visitorIdentity, int maxAge = 60)
    {
        visitorIdentity = string.IsNullOrWhiteSpace(visitorIdentity) ? _webHelper.GetClientIdentity() : visitorIdentity;
        if (string.IsNullOrWhiteSpace(visitorIdentity))
        {
            return null;
        }

        DateTime from = DateTime.UtcNow.AddSeconds(-maxAge);

        return await _db
            .Users.Where(x =>
                x.ClientIdentity == visitorIdentity && x.Username == null && x.Email == null &&
                x.LastActivityDateUtc >= from)
            .OrderByDescending(x => x.Id)
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync();
    }

    private static async Task<ClaimsPrincipal> EnsureAuthenticated(HttpContext httpContext)
    {
        var authResult = httpContext.Features.Get<IAuthenticateResultFeature>()
            ?.AuthenticateResult;

        if (authResult == null)
        {
            authResult = await httpContext.AuthenticateAsync();
        }

        return authResult.Principal ?? httpContext.User;
    }
}