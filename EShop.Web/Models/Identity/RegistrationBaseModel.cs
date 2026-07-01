using System.ComponentModel.DataAnnotations;
using EShop.Core.Platform.Configuration.Domain;
using EShop.Core.Platform.Identity.Configuration;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;

namespace EShop.Web.Models.Identity;

public abstract class RegistrationBaseModel
{
    [EmailAddress]
    public string Email { get; set; }
    public bool FirstNameRequired { get; set; }
    public string FirstName { get; set; }
    public bool LastNameRequired { get; set; }
    public string LastName { get; set; }
    public bool UsernameEnabled { get; set; }
    public string Username { get; set; }

    public bool GenderEnabled { get; set; }
    public string Gender { get; set; }
    public bool BirthdayEnabled { get; set; }
    public DateTime? BirthDay { get; set; }
}

public class RegistrationBaseModelValidator<T> : AbstractValidator<T>
    where T : RegistrationBaseModel
{
    public RegistrationBaseModelValidator(UserSettings userSettings)
    {
        
        if (userSettings.LastNameRequired)
        {
            RuleFor(x => x.LastName)
                .NotEmpty();
        }
    }
}

 