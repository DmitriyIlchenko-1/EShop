using EShop.Core.Platform.Configuration.Domain;
using EShop.Core.Platform.Identity.Domain;

namespace EShop.Core.Platform.Identity.Configuration;

public class UserSettings : ISettings
{
    public UserLoginType UserLoginType { get; set; } = UserLoginType.UsernameOrEmail;
    public bool FirstNameRequired { get; set; } = true;
    public bool LastNameRequired { get; set; }= true;
    public bool BirthdayEnabled { get; set; } = true;
    public bool GenderEnabled { get; set; } = true;
}