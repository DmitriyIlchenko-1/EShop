using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Extensions;
using EShop.Core.Platform.Identity.Services;
using EShop.Core.Platform.Web;
using EShop.Infrastructure.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace EShop.Core.Platform.Common;

public class CurrentUserWorkContextSource : ICurrentUserWorkContextSource
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;
    private readonly IWebHelper _webHelper;

    public CurrentUserWorkContextSource(IHttpContextAccessor httpContextAccessor, ApplicationDbContext db,
        IUserService userService, IWebHelper webHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _db = db;
        _userService = userService;
        _webHelper = webHelper;
    }

    private readonly static Func<DetectUserContext, Task<User?>>[] _userDetectors =
    [
        DetectAuthenticated,
        DetectGuest,
        DetectByClientIdentity
    ];


    public async Task<User?> ResolveCurrentUserAsync()
    {
        DetectUserContext context = new DetectUserContext()
        {
            HttpContext = _httpContextAccessor.HttpContext,
            WebHelper = _webHelper,
            Db = _db,
            UserService = _userService,
        };

        return await ResolveCurrentUserCoreAsync(context);
    }

    private async Task<User?> ResolveCurrentUserCoreAsync(DetectUserContext context)
    {
        User? user = null;

        foreach (var detector in _userDetectors)
        {
            user = await detector(context);

            if (user != null)
            {
                //TODO: ... if(is system account())

                if (!user.IsDeleted && user.Active)
                    break;
            }
        }

        if (user == null || (user.IsGuest()))
        {
            user = await CreateVisitorAsync();
        }

        return user;
    }

    private async Task<User> CreateVisitorAsync()
    {
        User user = await _userService.CreateVisitorUserAsync(_webHelper.GetClientIdentity(),
            user =>
            {
                user.LastIpAddress = _webHelper
                    .GetClientIpAddress()
                    .ToString();
                user.LastUserAgent = GetUserAgent();
                user.LastVisitedPage = _webHelper.GetCurrentPageUrl();
            });

        _userService.AppendVisitorCookie(user);
        return user;
    }

    private string GetUserAgent()
    {
        _ = _httpContextAccessor?.HttpContext?.Request.Headers.TryGetValue(HeaderNames.UserAgent,
            out var userAgent);
        return userAgent.ToString();
    }


    private static Task<User?> DetectAuthenticated(DetectUserContext context)
    {
        return context.UserService.GetAuthenticatedUserAsync();
    }


    /// <see href="https://stackoverflow.com/a/9358084/21915545"/>
    private static async Task<User?> DetectGuest(DetectUserContext context)
    {
        var visitorCookie = context.HttpContext?.Request.Cookies[CookieNames.Visitor];
        if (visitorCookie != null && Guid.TryParse(visitorCookie, out var userGuid))
        {
            context.UserGuid = userGuid;
            User? user = await context
                .Db.Users.Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserGuid == userGuid);

            if (user != null && !user.IsRegistered())
            {
                //TODO: add a traffic limitation check.
                return user;
            }
        }

        return null;
    }


    private static async Task<User?> DetectByClientIdentity(DetectUserContext context)
    {
        //Given these limitations, the five-minute window acts as a reasonable safeguard.
        //The system makes an assumption that if a request arrives without a cookie,
        //but with the same IP address and user agent as a very recent visitor, it is likely the same person.
        //The shorter the time frame, the higher the probability that this assumption is correct.
        //Extending this window would significantly increase the chances of "collisions,"
        //where different users are mistakenly treated as the same individual.
        //This helps to maintain a degree of data integrity for anonymous user tracking.

        var clientIdentity = context.WebHelper.GetClientIdentity();
        var user = await context.UserService.GetUserByIdentityAsync(clientIdentity, 300);

        if (user != null)
        {
            if (user.IsRegistered() || !user.IsGuest())
            {
                return null;
            }
            
            context.UserService.AppendVisitorCookie(user);
        }

        return user;
    }
}