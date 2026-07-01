using EShop.Core.Data;
using EShop.Core.Platform.Identity.Configuration;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Extensions;
using EShop.Infrastructure.Utilities;
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

    public override async Task<SignInResult> PasswordSignInAsync(string emailOrUsername, string password,
        bool isPersistent,
        bool lockoutOnFailure)
    {
        User? user;
        if (_userSettings.UserLoginType == UserLoginType.Email)
        {
            user = await UserManager.FindByEmailAsync(emailOrUsername);
        }
        else if (_userSettings.UserLoginType == UserLoginType.Username)
        {
            user = await UserManager.FindByNameAsync(emailOrUsername);
        }
        else
        {
            user = await UserManager.FindByEmailAsync(emailOrUsername) ??
                   await UserManager.FindByNameAsync(emailOrUsername);
        }

        if (user == null)
            return SignInResult.Failed;

        return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
    }

    public override async Task<SignInResult> PasswordSignInAsync(User user, string password, bool isPersistent,
        bool lockoutOnFailure)
    {
        if (user == null || user.IsDeleted)
            return SignInResult.Failed;

        //User has been blocked (Active = false) or hasn't confirmed their email
        if (!user.IsActive)
            return SignInResult.NotAllowed;

        if (!user.IsRegistered())
            return SignInResult.NotAllowed;

        var result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);

        if (result.Succeeded)
        {
            user.LastLoginDateUtc = DateTime.UtcNow;
            _db.Update(user);
            await _db.SaveChangesAsync();
        }

        return result;
    }

    public override Task<bool> CanSignInAsync(User user)
    {
        Guard.NotNull(user);
        
        if (!user.IsActive)
        {
            return Task.FromResult(false);
        }
        
        return base.CanSignInAsync(user);
    }
}