using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace EShop.Core.Platform.Common;

public class DefaultWorkContext : IWorkContext
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;
    private readonly IUserService _userService;
    private readonly ICurrentUserWorkContextSource _currentUserWorkContextSource;
    private User? _currentUser;

    public DefaultWorkContext(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager, IUserService userService,
        ICurrentUserWorkContextSource currentUserWorkContextSource)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _userService = userService;
        _currentUserWorkContextSource = currentUserWorkContextSource;
    }


    public User CurrentUser
    {
        get
        {
            if (_currentUser == null)
            {
                var user = _currentUserWorkContextSource
                    .ResolveCurrentUserAsync()
                    .GetAwaiter()
                    .GetResult();
                return _currentUser = user;
            }

            return _currentUser;
        }
    }
}