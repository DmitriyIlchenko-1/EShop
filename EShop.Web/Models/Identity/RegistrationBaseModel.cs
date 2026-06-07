using System.ComponentModel.DataAnnotations;
using EShop.Core.Platform.Configuration.Domain;
using EShop.Core.Platform.Identity.Configuration;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;

namespace EShop.Web.Models.Identity;

public abstract class RegistrationBaseModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; }

    public bool FirstNameRequired { get; set; }

    [Required(ErrorMessage = "Firstname is required")]
    public string FirstName { get; set; }
    public bool LastNameRequired { get; set; }

    [Required(ErrorMessage = "Lastname is required")]
    public string LastName { get; set; }

    public string Username { get; set; }

    public string Gender { get; set; }
    public bool BirthdayEnabled { get; set; }
    public DateTime? BirthDay { get; set; }
}

public class RegistrationBaseModelValidator<T> : AbstractValidator<T>
    where T : RegistrationBaseModel
{
    public RegistrationBaseModelValidator(UserSettings userSettings)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        if (userSettings.FirstNameRequired)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty();
        }

        if (userSettings.LastNameRequired)
        {
            RuleFor(x => x.LastName)
                .NotEmpty();
        }
    }
}

 