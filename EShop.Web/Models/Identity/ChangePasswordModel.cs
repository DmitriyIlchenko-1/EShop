using System.ComponentModel.DataAnnotations;
using EShop.Web.Common.Models;
using FluentValidation;

namespace EShop.Web.Models.Identity;

public class ChangePasswordModel : BaseModel
{
    [DataType(DataType.Password)]
    public string OldPassword { get; set; }
    
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }
    
    [DataType(DataType.Password)]
    public string RepeatNewPassword { get; set; }

    public string Result { get; set; }
 
}

public class ChangePasswordModelValidator : AbstractValidator<ChangePasswordModel>
{
    public ChangePasswordModelValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .Equal(x => x.RepeatNewPassword)
            .WithMessage("Passwords do not match.");
    }
}

