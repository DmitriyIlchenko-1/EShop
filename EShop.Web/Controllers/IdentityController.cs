using System.Globalization;
using System.Security.Claims;
using EShop.Core.Data;
using EShop.Core.Data.Extensions;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Configuration;
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
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace EShop.Web.Controllers;
/*
 * TODO:
 * 1. Deal with security stamps because right now we're using a workaround.
 * 2. Add password validator for registration.
 * 5. Style the summary list on top of the from.
 * 6. Learn and apply autocomplete.
 * 7. Make email validation stricter at the backend (don't touch jQuery for this)
 * 8. Write some custom jQuery JS to prevent 'valid' from applying to valid form controls until later on when the form is first submitted.
 * It is so that the success styling doesn't get applied when nothing has failed the validation yet.
 *9. Add readonly styles :readonly
 * 10. style alert in ChangePassword.cshtml
 */
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
    


    public IdentityController(SignInManager<User> signInManager, UserManager<User> userManager,
        IWorkContext workContext, IActivityLogger activityLogger, INotificationManager notifyManager,
        ApplicationDbContext dbContext, UserSettings userSettings, IEmailService emailService
        )
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _workContext = workContext;
        _activityLogger = activityLogger;
        _notifyManager = notifyManager;
        _dbContext = dbContext;
        _userSettings = userSettings;
        _emailService = emailService;
       
    }


    #region Registration

    [HttpGet("register")]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : string.Empty;
        var model = new ApplicationRegistrationModel();
        PrepareRegisterModel(model);
        return View(model);
    }

    private void PrepareRegisterModel(RegistrationBaseModel model)
    {
        model.UsernameEnabled = _userSettings.UserLoginType != UserLoginType.Email;
        model.FirstNameRequired = _userSettings.FirstNameRequired;
        model.LastNameRequired = _userSettings.LastNameRequired;
        model.BirthdayEnabled = _userSettings.BirthdayEnabled;
        model.GenderEnabled = _userSettings.GenderEnabled;
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

        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = string.Empty;
        }

        if (ModelState.IsValid)
        {
            bool succeeded = false;
            var oldFirstName = user.FirstName;
            var oldLastName = user.LastName;
            var oldEmail = user.Email;
            var oldCreatedOn = user.CreatedOnUtc;
            var oldLastActivityDate = user.LastActivityDateUtc;
            var oldActive = user.IsActive;
            var oldUsername = user.Username;

            user.Username = model.Username != null ? model.Username.Trim() : model.Email.Trim();
            user.Email = model.Email.Trim();
            //TODO: add a possibility to change registration types, in which case Active should be set to true if email confirmation isn't required, which, right now, it always is.
            user.IsActive = false;
            user.CreatedOnUtc = DateTime.UtcNow;
            user.LastActivityDateUtc = DateTime.UtcNow;
            //TODO:
            user.SecurityStamp = string.Empty;

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
                    user.Username = oldUsername;
                    user.LastName = oldLastName;
                    user.Email = oldEmail;
                    user.CreatedOnUtc = oldCreatedOn;
                    user.LastActivityDateUtc = oldLastActivityDate;
                    user.IsActive = oldActive;
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

        ViewData["ReturnUrl"] = returnUrl;
        PrepareRegisterModel(model);
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
    public async Task<IActionResult> ConfirmEmail(string token, string email)
    {
        User? user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            _notifyManager.AddError("Email or Confirmation Tokes are invalid.");
            return RedirectToAction("Index", "Home");
        }


        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            ViewBag.ConfirmationResult = "Your email has already been confirmed.";
        }

        var confirm = await _userManager.ConfirmEmailAsync(user, UrlExtensions.Base64UrlDecode(token));
        if (!confirm.Succeeded)
        {
            _notifyManager.AddError("Email or Confirmation Tokes are invalid.");
            return RedirectToAction("Index", "Home");
        }

        ViewBag.ConfirmationResult = "Your email has been confirmed. You can now sign in.";
        return View();
    }

    #endregion


    #region Login

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");
        var model = new LoginModel()
        {
            UserLoginType = _userSettings.UserLoginType
        };
        return View(model);
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel loginModel, string? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            User user;
            if (loginModel.UserLoginType == UserLoginType.Email)
            {
                user = await _userManager.FindByEmailAsync(loginModel.Email.TrimSafe());
            }
            else if (loginModel.UserLoginType == UserLoginType.Username)
            {
                user = await _userManager.FindByNameAsync(loginModel.Username.TrimSafe());
            }
            else
            {
                user = await _userManager.FindByEmailAsync(loginModel.UsernameOrEmail.TrimSafe()) ??
                       await _userManager.FindByNameAsync(loginModel.UsernameOrEmail.TrimSafe());
            }

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
                    if (!user.EmailConfirmed && !user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "You have to confirm your email first, before you can log in.");
                    }
                    else if (logInResult.RequiresTwoFactor)
                    {
                        //TODO ...
                    }
                    else
                    {
                       ModelState.AddModelError(string.Empty, "The credentials are invalid");
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "The credentials are invalid");
            }
        }

        ViewData["ReturnUrl"] = returnUrl;
        loginModel.UserLoginType = _userSettings.UserLoginType;
        return View(loginModel);
    }

    #endregion

    #region Logout

    [HttpGet("logout")]
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

                    //A new user, have them complete the registration to fill out the missing fields e.g. Birthday
                    if (user == null)
                    {
                        return RedirectToAction(nameof(ExternalAccountRegisterNew),
                            new
                            {
                                returnUrl
                            });
                    }

                    //The user has used a different external provider, though, they already have an account in the system
                    await _userManager.AddLoginAsync(user, externalLoginInfo);
                }
            }

            //TODO: Do we need to bypass Two Factor? 
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
                // TODO: ...
            }
        }

        _notifyManager.AddError("Something went wrong during the authentication. Try again later.");
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

        PrepareRegisterModel(model);
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

        ClaimsPrincipal principal = info.Principal;
        if (ModelState.IsValid)
        {
          
            User user = new User
            {
                Username = principal.FindFirstValue(ClaimTypes.Email),
                Email = principal.FindFirstValue(ClaimTypes.Email),
                FirstName = principal.FindFirstValue(ClaimTypes.GivenName),
                LastName = principal.FindFirstValue(ClaimTypes.Surname) ?? model.LastName,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                IsActive = true,
                //we assume the user's email is confirmed because we rely on their external provider to verify it.
                EmailConfirmed = true
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


        PrepareRegisterModel(model);
        model.Email = principal.FindFirstValue(ClaimTypes.Email);
        model.FirstName = principal.FindFirstValue(ClaimTypes.GivenName);
        model.LastName = principal.FindFirstValue(ClaimTypes.Surname);
        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }


    public IActionResult ExternalError(string provider, string errorType, string error)
    {
        if (provider.HasValue() || error.HasValue())
        {
            Logger.LogError(
                "Error from an external authentication provider. Provider: {provider}, Error Type: {errorType}, Error: {error}",
                provider,
                errorType.EmptyIfNull(),
                error);
        }

        var msg = errorType == "access_denied" ? error : "Something went wrong during authentication. Try again later.";
        _notifyManager.AddError(msg);
        return RedirectToAction(nameof(Login));
    }

    #endregion


    #region Change password

    [Authorize(Roles = "Registered")]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordModel());
    }

    [Authorize(Roles = "Registered")]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        if (ModelState.IsValid)
        {
            var passwordResult = await _userManager.ChangePasswordAsync(
                _workContext.CurrentUser,
                model.OldPassword,
                model.NewPassword);

            if (passwordResult.Succeeded)
            {
                model.Result = "Password changed successfully";
            }
            else
            {
                AddModelStateErrors(passwordResult);
            }
        }

        return View(model);
    }

    #endregion

    #region Password Reset

    [HttpGet("/password-reset")]
    public IActionResult PasswordReset()
    {
        return View(new PasswordResetModel());
    }

    [HttpPost("/password-reset")]
    public async Task<IActionResult> PasswordReset(PasswordResetModel model)
    {
        if (ModelState.IsValid)
        {
            User user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && user.IsActive)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _emailService.SendResetPassword(user, UrlExtensions.Base64UrlEncode(token));
            }

            model.ResultMessage = "The e-mail has been sent";
        }

         

        return View(model);
    }

    [HttpGet("password-reset-confirm")]
    public IActionResult PasswordResetConfirm(string token, string email)
    {
        if (token.IsEmpty() || email.IsEmpty())
        {
            return RedirectToAction("Index", "Home");
        }

        var model = new PasswordResetConfirmationModel()
        {
            Token = token,
            Email = email
        };

        return View(model);
    }

    [HttpPost("password-reset-confirm")]
    public async Task<IActionResult> PasswordResetConfirm(PasswordResetConfirmationModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        foreach (var validator in _userManager.PasswordValidators)
        {
            AddModelStateErrors(await validator.ValidateAsync(_userManager, user, model.NewPassword));
        }
        
        if (ModelState.IsValid)
        {
            var resetResult = await _userManager.ResetPasswordAsync(user, UrlExtensions.Base64UrlDecode(model.Token), model.NewPassword);
            if (resetResult.Succeeded)
            {
                model.IsResultSuccess = true;
                model.ResultMessage = "Your password has been changed";
            }
            else
            {
                resetResult.Errors.Each(x => _notifyManager.AddError(x.Description));
            }
        }

        return View(model);
    }

    #endregion

    [HttpGet("access-denied")]
    public IActionResult AccessDenied(string returnUrl = null)
    {
        HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    #region Helpers

    private void MapRegisterModelToUser(RegistrationBaseModel model, User user)
    {
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        if (_userSettings.BirthdayEnabled && model.BirthDay.HasValue)
        {
            user.BirthDate = model.BirthDay;
        }

        if (_userSettings.GenderEnabled)
        {
            user.Gender = model.Gender;
        }
    }

    protected virtual async Task<IActionResult> FinalizeUserRegistrationAsync(User user, string returnUrl)
    {
        user.ClientIdentity = null;

        //roles
        await _userManager.AddToRoleAsync(user, UserRoleNameConstants.Registered);
        await _userManager.RemoveFromRoleAsync(user, UserRoleNameConstants.Guest);


        ExternalLoginInfo? externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo != null)
        {
            return RedirectToLocal(returnUrl);
        }
        else
        {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendEmailConfirmation(user, UrlExtensions.Base64UrlEncode(token));
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