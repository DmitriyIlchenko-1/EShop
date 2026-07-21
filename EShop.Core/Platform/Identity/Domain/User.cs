using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EShop.Core.Common.Domain;
using EShop.Core.Data.Cart.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Platform.Identity.Domain;

internal class UserMap : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder
            .HasMany(x => x.Addresses)
            .WithMany()
            .UsingEntity<UserAddress>();
    }
}

public class User : BaseEntity
{
     
    public Guid UserGuid { get; set; } = Guid.NewGuid();
    public bool IsActive { get; set; }
    public bool IsSystemAccount { get; set; }
    public string SystemName { get; set; }
    public string? SecurityStamp { get; set; }
    [StringLength(50)] public string FirstName { get; set; }

    [StringLength(50)] public string LastName { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? BirthDate { get; set; }

    [StringLength(50)] public string Gender { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string ExtensionData { get; set; }

    [StringLength(32)] public string ClientIdentity { get; set; }

    [StringLength(100)] public string LastIpAddress { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string LastUserAgent { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string LastUserDeviceType { get; set; }

    [StringLength(2048)] public string LastVisitedPage { get; set; }
    public string DiscountCouponCode { get; set; }

    public string? Username { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    //Currently this is essentially password hash, though in future I might add other password types, in which case there's going to be other properties indicating what this property contains.
    public string? Password { get; set; }


    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }
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

    public ICollection<Address> Addresses { get; set; } = [];
    
     
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
}