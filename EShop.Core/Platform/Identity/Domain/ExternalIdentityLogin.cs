using EShop.Infrastructure.Domain;
using Microsoft.AspNetCore.Identity;

namespace EShop.Core.Platform.Identity.Domain;

public class ExternalIdentityLogin : BaseEntity
{
    public string LoginProvider { get; set; }
    public string ProviderKey { get; set; }
    public string? ProviderDisplayName { get; set; }
    public int UserId { get; set; }
}