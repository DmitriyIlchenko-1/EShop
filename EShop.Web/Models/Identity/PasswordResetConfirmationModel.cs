using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace EShop.Web.Models.Identity;

public class PasswordResetConfirmationModel
{
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    public string Token { get; set; }
    
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }
    
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }

    public bool IsResultSuccess { get; set; }
    public string ResultMessage { get; set; }
}

public class PasswordResetConfirmationModelValidator : AbstractValidator<PasswordResetConfirmationModel>
{
    public PasswordResetConfirmationModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.Token)
            .NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .Equal(x => x.ConfirmPassword);
    }
}