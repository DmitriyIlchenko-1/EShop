using EShop.Core.Platform.Configuration.Domain;
using EShop.Core.Platform.Identity.Domain;

namespace EShop.Core.Platform.Identity.Configuration;

public class UserSettings : ISettings
{
    public UserLoginType UserLoginType { get; set; }
}