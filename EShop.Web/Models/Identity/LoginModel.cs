using System.ComponentModel.DataAnnotations;
using EShop.Core.Platform.Identity.Configuration;
using EShop.Core.Platform.Identity.Domain;
using FluentValidation;
using Microsoft.VisualBasic.CompilerServices;

namespace EShop.Web.Models.Identity;

public class LoginModel
{
    public UserLoginType UserLoginType { get; set; }
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    public string Username { get; set; }
    public string UsernameOrEmail { get; set; }
    [DataType(DataType.Password)]
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}


public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator(UserSettings settings)
    {
        switch (settings.UserLoginType)
        {
            case UserLoginType.Email:
                RuleFor(x => x.Email)
                    .NotEmpty()
                    .EmailAddress();
                break;
            case UserLoginType.Username:
                RuleFor(x => x.Username)
                    .NotEmpty();
                break;
            case UserLoginType.UsernameOrEmail:
                RuleFor(x => x.UsernameOrEmail)
                    .NotEmpty();
                break;
        }

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}