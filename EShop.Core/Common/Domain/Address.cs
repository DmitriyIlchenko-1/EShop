using System.ComponentModel.DataAnnotations;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Common.Domain;

internal class AddressMap : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder
            .HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class Address : BaseEntity
{
    [StringLength(200)] public string FirstName { get; set; }
    [StringLength(200)] public string LastName { get; set; }
    [StringLength(100)] public string PhoneNumber { get; set; }
    [StringLength(500)] public string AddressLine1 { get; set; }
    [StringLength(500)] public string AddressLine2 { get; set; }
    [StringLength(100)] public string ZipCode { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public int? CityId { get; set; }
    public City City { get; set; }

    public override bool Equals(BaseEntity other)
    {
        return base.Equals(other) ||
               other is Address otherAddress && (otherAddress.AddressLine1 == AddressLine1
                                                                    && otherAddress.AddressLine2 == AddressLine2
                                                                    && otherAddress.FirstName == FirstName &&
                                                                    otherAddress.LastName == LastName &&
                                                                    otherAddress.PhoneNumber == PhoneNumber
                                                                    && otherAddress.ZipCode == ZipCode &&
                                                                    otherAddress.CityId == CityId);
    }

    public override string ToString()
    {
        return $"{FirstName}, {LastName}. {AddressLine1}.";
    }
}