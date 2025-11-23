using System.ComponentModel.DataAnnotations;
using EShop.Core.Common.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Platform.Identity.Domain;

internal class UserMap : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class User : IdentityUser<int>, IEntityWithTypedId<int>
{
    public Guid UserGuid { get; set; } = Guid.NewGuid();
    public bool Active { get; set; }

    [StringLength(50)] public string FirstName { get; set; }

    [StringLength(50)] public string LastName { get; set; }
    public DateTime? BirthDate { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Gender { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string ExtensionData { get; set; }

    [StringLength(32)] public string ClientIdentity { get; set; }

    [StringLength(100)] public string LastIpAddress { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string LastUserAgent { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string LastUserDeviceType { get; set; }

    [StringLength(2048)] public string LastVisitedPage { get; set; }
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LatestUpdateOnUtc { get; set; }
    public DateTime LastActivityDateUtc { get; set; }
    public DateTime? LastLoginDateUtc { get; set; }
    public bool IsDeleted { get; set; }

    //DefaultBillingAddressId
    public int? BillingAddressId { get; set; }

    //DefaultShippingAddressId
    public int? ShippingAddressId { get; set; }

    public Address BillingAddress { get; set; }

    public Address ShippingAddress { get; set; }

    public ICollection<UserAddress> UserAddresses { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
}