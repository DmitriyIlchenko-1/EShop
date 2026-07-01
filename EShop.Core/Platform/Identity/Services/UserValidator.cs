using EShop.Core.Platform.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace EShop.Core.Platform.Identity.Services;

public class UserValidator : PasswordValidator<User>, IUserValidator<User>
{
    public async Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user)
    {
        throw new NotImplementedException();
    }
}