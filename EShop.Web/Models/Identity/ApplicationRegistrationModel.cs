using System.ComponentModel.DataAnnotations;
using EShop.Core.Platform.Identity.Configuration;
using FluentValidation;

namespace EShop.Web.Models.Identity;

public class ApplicationRegistrationModel : RegistrationBaseModel
{
     
    [DataType(DataType.Password)]
    public string Password { get; set; }
  
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }
}

public class ApplicationRegistrationModelValidator : RegistrationBaseModelValidator<ApplicationRegistrationModel>
{
    public ApplicationRegistrationModelValidator(UserSettings userSettings) : base(userSettings)
    { 
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        if (userSettings.FirstNameRequired)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty();
        }

        RuleFor(x => x.Password)
            .NotEmpty();
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().Equal(x => x.Password);
        
    }
}