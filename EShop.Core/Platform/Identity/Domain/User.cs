using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EShop.Core.Checkout.Orders.Domain;
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
            .HasOne(c => c.BillingAddress)
            .WithOne();

        builder
            .HasOne(c => c.ShippingAddress)
            .WithOne();
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

    private Address _billingAddress;

    public Address BillingAddress
    {
        get => _billingAddress ?? LazyLoader.Load(this, ref _billingAddress);
        set => _billingAddress = value;
    }

    private Address _shippingAddress;

    public Address ShippingAddress
    {
        get => _shippingAddress ?? LazyLoader.Load(this, ref _shippingAddress);
        set => _shippingAddress = value;
    }


    private ICollection<Address> _addresses;

    public ICollection<Address> Addresses
    {
        get => _addresses ?? LazyLoader.Load(this, ref _addresses) ?? (_addresses ??= new HashSet<Address>());
        set => _addresses = value;
    }

    private ICollection<UserRole> _userRoles;

    public ICollection<UserRole> UserRoles
    {
        get => _userRoles ?? LazyLoader.Load(this, ref _userRoles) ?? (_userRoles ??= new HashSet<UserRole>());
        set => _userRoles = value;
    }

    private ICollection<ShoppingCartItem> _shoppingCartItems;

    public ICollection<ShoppingCartItem> ShoppingCartItems
    {
        get => _shoppingCartItems ?? LazyLoader.Load(this, ref _shoppingCartItems) ?? (_shoppingCartItems ??= new HashSet<ShoppingCartItem>());
        set => _shoppingCartItems = value;
    }


    private ICollection<Order> _orders;

    public ICollection<Order> Orders
    {
        get => _orders ?? LazyLoader.Load(this, ref _orders) ?? (_orders ??= new HashSet<Order>());
        set => _orders = value;
    }

    public void RemoveAddress(Address address)
    {
        if (Addresses.Contains(address))
        {
            if (ShippingAddress == address)
            {
                ShippingAddress = null;
            }

            Addresses.Remove(address);
        }
    }
}