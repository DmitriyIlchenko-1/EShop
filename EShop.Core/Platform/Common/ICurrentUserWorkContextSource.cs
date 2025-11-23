using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace EShop.Core.Platform.Common;

public interface ICurrentUserWorkContextSource
{
    Task<User?> ResolveCurrentUserAsync();
}