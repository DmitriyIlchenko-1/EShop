using EShop.Infrastructure.Domain;
using Microsoft.AspNetCore.Identity;

namespace EShop.Core.Platform.Identity.Domain;

public class Role :BaseEntity 
{
    public string Name { get; set; }
    public bool Active { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } 
    public bool IsSystemRole { get; set; }
}