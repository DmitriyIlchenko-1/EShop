using System.ComponentModel.DataAnnotations;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Domain;
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

        //We can't set both to 'SetNull' explicitly because we'll get an exception.
        //The default (implicit) behaviour for nullable structures is going to be the same, though.
        builder
            .HasOne(x => x.District)
            .WithMany()
            .HasForeignKey(x => x.DistrictId);


    }
}

public class Address : BaseEntity
{
    [StringLength(200)]
    public string FirstName { get; set; }
    [StringLength(200)]
    public string LastName { get; set; }
    [StringLength(100)]
    public string PhoneNumber { get; set; }
    [StringLength(500)]
    public string AddressLine1 { get; set; }
    [StringLength(500)]
    public string AddressLine2 { get; set; }
    [StringLength(100)]
    public string ZipCode { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public int? CityId { get; set; }
    public City City { get; set; }
    public int? DistrictId { get; set; }
    public District District { get; set; }
}