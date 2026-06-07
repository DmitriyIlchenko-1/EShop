using System.Globalization;
using System.Security.Claims;
using EShop.Core.Data;
using EShop.Core.Data.Extensions;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Extensions;
using EShop.Core.Platform.Logging.Services;
using EShop.Core.Platform.Web;
using EShop.Infrastructure.Email;
using EShop.Infrastructure.Extensions;
using EShop.Web.Common.Controllers;
using EShop.Web.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UserSettings = EShop.Core.Platform.Identity.Configuration.UserSettings;

namespace EShop.Web.Controllers;

public class IdentityController : EShopBaseController
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IWorkContext _workContext;
    private readonly IActivityLogger _activityLogger;
    private readonly INotificationManager _notifyManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly UserSettings _userSettings;
    private readonly IEmailService _emailService;
    private readonly LinkGenerator _linkGenerator;


    public IdentityController(SignInManager<User> signInManager, UserManager<User> userManager,
        IWorkContext workContext, IActivityLogger activityLogger, INotificationManager notifyManager,
        ApplicationDbContext dbContext, UserSettings userSettings, IEmailService emailService, LinkGenerator linkGenerator)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _workContext = workContext;
        _activityLogger = activityLogger;
        _notifyManager = notifyManager;
        _dbContext = dbContext;
        _userSettings = userSettings;
        _emailService = emailService;
        _linkGenerator = linkGenerator;
    }


    #region Registration

    [HttpGet("register")]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : string.Empty;
        var model = new ApplicationRegistrationModel();
        PrepareRegisterModelAsync(model);
        return View(model);
    }

    private void PrepareRegisterModelAsync(ApplicationRegistrationModel model)
    {
        model.FirstNameRequired = _userSettings.FirstNameRequired;
        model.LastNameRequired = _userSettings.LastNameRequired;
        model.BirthdayEnabled = _userSettings.BirthdayEnabled;
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(ApplicationRegistrationModel model, string returnUrl = null)
    {
        User user = _workContext.CurrentUser;
        if (user.IsRegistered())
        {
            return RedirectToAction(nameof(RegisterResult),
                new
                {
                    message = "You've already registered. Use your email and password to log in.",
                    returnUrl
                });
        }

        foreach (var validator in _userManager.PasswordValidators)
        {
            AddModelStateErrors(await validator.ValidateAsync(_userManager, user, model.Password));
        }

        if (ModelState.IsValid)
        {
            bool succeeded = false;
            var oldFirstName = user.FirstName;
            var oldLastName = user.LastName;
            var oldEmail = user.Email;
            var oldCreatedOn = user.CreatedOnUtc;
            var oldLastActivityDate = user.LastActivityDateUtc;

            user.UserName = model.Username != null ? model.Username.Trim() : model.Email.Trim();
            user.Email = model.Email.Trim();
            user.CreatedOnUtc = DateTime.UtcNow;
            user.LastActivityDateUtc = DateTime.UtcNow;

            try
            {
                var identityResult = await _userManager.UpdateAsync(user);
                if (identityResult.Succeeded)
                {
                    var passwordResult = await _userManager.AddPasswordAsync(user, model.Password);
                    succeeded = passwordResult.Succeeded;
                    AddModelStateErrors(passwordResult);
                }

                AddModelStateErrors(identityResult);
            }
            finally
            {
                if (!succeeded)
                {
                    user.FirstName = oldFirstName;
                    user.LastName = oldLastName;
                    user.Email = oldEmail;
                    user.CreatedOnUtc = oldCreatedOn;
                    user.LastActivityDateUtc = oldLastActivityDate;
                    await _dbContext.SaveChangesAsync();
                }
            }

            if (succeeded)
            {
                MapRegisterModelToUser(model, user);
                await _dbContext.SaveChangesAsync();
                return await FinalizeUserRegistrationAsync(user, returnUrl);
            }
        }

        return View(model);
    }

    public IActionResult RegisterResult(string message, string returnUrl)
    {
        ViewData["Message"] = message ?? "Thank you for registering.";
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
        return View();
    }

    #endregion

    #region Confim Email

    [HttpGet(nameof(ConfirmEmail))]
    public async Task<IActionResult> ConfirmEmail(string code, string email)
    {
        User? user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            _notifyManager.AddError("Email or Confirmation Tokes are invalid.");
            return RedirectToAction("Index", "Home");
        }

        var confirm = await _userManager.ConfirmEmailAsync(user, code);
        if (!confirm.Succeeded)
        {
            _notifyManager.AddError("Email or Confirmation Tokes are invalid.");
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    #endregion


    #region Login

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel loginModel, string? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user != null)
            {
                var logInResult = await _signInManager.PasswordSignInAsync(user,
                    loginModel.Password,
                    loginModel.RememberMe,
                    lockoutOnFailure: false);

                if (logInResult.Succeeded)
                {
                    await FinalizeUserLoginAsync(_workContext.CurrentUser, user);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    if (!user.EmailConfirmed && !user.Active)
                    {
                        _notifyManager.AddInfo("You have to confirm your first, before you can log in.");
                    }

                    if (logInResult.RequiresTwoFactor)
                    {
                        //TODO ...
                    }

                    if (user.EmailConfirmed && !user.Active)
                    {
                        _notifyManager.AddWarning(
                            "You account has been deactivated. Contact the owner of the website to find out why.");
                        RedirectToAction(nameof(Login));
                    }
                }
            }

            _notifyManager.AddError("The credentials are invalid", false);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    #endregion

    #region Logout

    public async Task<ActionResult> Logout()
    {
        _activityLogger.InsertActivity(KnownActivityLogType.Logout,
            KnownActivityFormats.Logout);

        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    #endregion

    #region External Login

    [HttpPost]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string provider, string returnUrl = "/")
    {
        string redirectUrl = Url.Action(nameof(CorrelateExternalAuth), values: new { returnUrl });
        AuthenticationProperties properties =
            _signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CorrelateExternalAuth(string returnUrl = "/")
    {
        ExternalLoginInfo externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo != null)
        {
            User user =
                await _userManager.FindByLoginAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey);

            if (user == null)
            {
                string externalEmail = externalLoginInfo.Principal.FindFirst(ClaimTypes.Email)
                    ?.Value ?? string.Empty;

                if (!externalEmail.IsEmpty())
                {
                    user = await _userManager.FindByEmailAsync(externalEmail);

                    if (user == null)
                    {
                        return RedirectToAction(nameof(ExternalAccountRegisterNew),
                            new
                            {
                                returnUrl
                            });
                    }

                    await _userManager.AddLoginAsync(user, externalLoginInfo);
                }
            }

            //TODO: make it possible for the User to chose if the session should be persistent
            var signInResult = await _signInManager.ExternalLoginSignInAsync(externalLoginInfo.LoginProvider,
                externalLoginInfo.ProviderKey,
                true,
                false);

            if (signInResult.Succeeded)
            {
                await FinalizeUserLoginAsync(_workContext.CurrentUser, user);
                return RedirectToLocal(returnUrl);
            }
            else if (signInResult.RequiresTwoFactor)
            {
                // ...
            }

            //If the user isn't active, they can't log in because WorkContext won't let them. 
        }

        _notifyManager.AddError("Something went wrong during authentication. Try again later.");
        return RedirectToAction(nameof(Login));
    }


    public async Task<IActionResult> ExternalAccountRegisterNew(string returnUrl = "/")
    {
        ExternalLoginInfo? info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _notifyManager.AddError("Something went wrong during authentication. Try again later.");
            return RedirectToAction(nameof(Login));
        }

        var claimPrincipal = info.Principal;
        var model = new ExternalRegistrationModel()
        {
            Email = claimPrincipal.FindFirstValue(ClaimTypes.Email),
            FirstName = claimPrincipal.FindFirstValue(ClaimTypes.GivenName),
            LastName = claimPrincipal.FindFirstValue(ClaimTypes.Surname)
        };
        ViewData["ReturnUrl"] = returnUrl;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ExternalAccountRegisterNew(ExternalRegistrationModel model,
        [FromQuery] string returnUrl = "/")
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _notifyManager.AddError("Something went wrong during authentication. Try again later.");
            return RedirectToAction(nameof(Login));
        }

        if (ModelState.IsValid)
        {
            ClaimsPrincipal principal = info.Principal;
            User user = new User
            {
                Email = principal.FindFirstValue(ClaimTypes.Email),
                FirstName = principal.FindFirstValue(ClaimTypes.GivenName),
                LastName = principal.FindFirstValue(ClaimTypes.Surname),
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                Active = true
            };
            MapRegisterModelToUser(model, user);

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);
                    await FinalizeUserLoginAsync(_workContext.CurrentUser, user);
                    return await FinalizeUserRegistrationAsync(user, returnUrl);
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }


        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }


    public async Task<IActionResult> ExternalError(string provider, string error)
    {
        //TODO: add loging 
        //TODO: let the user know they have denied access to their data. 
        _notifyManager.AddError("Something went wrong during authentication. Try again later.");
        return RedirectToAction(nameof(Login));
    }

    #endregion


    #region Password Reset

    [HttpGet("/password-reset")]
    public IActionResult PasswordReset()
    {
        return View();
    }

    // [HttpPost("password-reset")]
    // public async Task<IActionResult> PasswordReset(PasswordResetModel model)
    // {
    //     if (ModelState.IsValid)
    //     {
    //         User user = await _userManager.FindByEmailAsync(model.Email);
    //         if (user != null && user.Loc)
    //         {
    //             
    //         }
    //     }
    // }

    #endregion

    #region Helpers

    private void MapRegisterModelToUser(RegistrationBaseModel model, User user)
    {
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Gender = model.Gender;
        if (_userSettings.BirthdayEnabled && model.BirthDay.HasValue)
        {
            user.BirthDate = model.BirthDay;
        }
    }

    private async Task<IActionResult> FinalizeUserRegistrationAsync(User user, string returnUrl)
    {
        user.ClientIdentity = null;

        //roles
        await _userManager.AddToRoleAsync(user, UserRoleNameConstants.Registered);
        await _userManager.RemoveFromRoleAsync(user, UserRoleNameConstants.Guest);


        ExternalLoginInfo? externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo != null)
        {
            string redirectUrl = Url.Action(nameof(RegisterResult));
            if (Url.IsLocalUrl(returnUrl))
            {
                redirectUrl = $"{redirectUrl}?returnUrl={Uri.EscapeDataString(returnUrl)}";
            }

            return Redirect(redirectUrl);
        }
        else
        {
            //email confirmation
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var msg = string.Format(CultureInfo.InvariantCulture, UserConstantTemplates.EmailConfirmation, token);
            await _emailService.SendEmailConfirmation(user, msg);
            var path = _linkGenerator.GetPathByAction(HttpContext, nameof(ConfirmEmail), values: new {  token });
            return RedirectToAction(nameof(RegisterResult),
                new
                {
                    message =
                        "Thank you for registering. A link has been sent to your email address. Go to your email provider and follow the instructions to activate your account."
                });
        }
    }


    private async Task FinalizeUserLoginAsync(User guest, User user)
    {
        //TODO:   await MigrateFromGuestAsync(_workContext.CurrentUser, user);
        //TODO: fire an event;

        ExternalLoginInfo? externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo != null)
        {
            _activityLogger.InsertActivity(KnownActivityLogType.Login,
                KnownActivityFormats.ExternalLogin,
                new object[]
                {
                    externalLoginInfo.LoginProvider,
                    user.Email,
                    user.Id
                });
        }
        else
        {
            _activityLogger.InsertActivity(KnownActivityLogType.Login,
                KnownActivityFormats.Login,
                new object[]
                {
                    user.Email,
                    user.Id
                });
        }
    }


    private void AddModelStateErrors(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            result
                .Errors.Select(x => x.Description)
                .Distinct()
                .Each(x => ModelState.AddModelError(string.Empty, x));
        }
    }

    #endregion
}