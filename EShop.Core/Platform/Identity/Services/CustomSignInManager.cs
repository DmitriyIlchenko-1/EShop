using EShop.Core.Data;
using EShop.Core.Platform.Identity.Configuration;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EShop.Core.Platform.Identity.Services;

public class CustomSignInManager : SignInManager<User>
{
    private readonly ApplicationDbContext _db;
    private readonly UserSettings _userSettings;


    public CustomSignInManager(UserManager<User> userManager, IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<User> claimsFactory, IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<User>> logger, IAuthenticationSchemeProvider schemes,
        IUserConfirmation<User> confirmation, ApplicationDbContext db, UserSettings userSettings) : base(
        userManager,
        contextAccessor,
        claimsFactory,
        optionsAccessor,
        logger,
        schemes,
        confirmation)
    {
        _db = db;
        _userSettings = userSettings;
    }

    public async Task<SignInResult> PasswordSignInAsync(string email, string password,
        bool isPersistent,
        bool lockoutOnFailure)
    {
        User? user = await UserManager.FindByEmailAsync(email);

        if (user == null)
            return SignInResult.Failed;

        return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
    }

    public override async Task<SignInResult> PasswordSignInAsync(User user, string password, bool isPersistent,
        bool lockoutOnFailure)
    {
        if (user == null || user.IsDeleted)
            return SignInResult.Failed;

        if (!user.Active)
            return SignInResult.NotAllowed;

        if (!user.IsRegistered())
            return SignInResult.NotAllowed;

        var result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);

        if (result.Succeeded)
        {
            user.LastLoginDateUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return result;
    }
}