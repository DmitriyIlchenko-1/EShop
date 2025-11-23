using System.ComponentModel.DataAnnotations;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Common.Domain;

internal class DistrictMap : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder
            .HasOne(x => x.City)
            .WithMany(x => x.Districts)
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class District : BaseEntity, IDisplayOrder
{
    [StringLength(200)] 
    public string Name { get; set; }
    public int CityId { get; set; }
    public City City { get; set; }
    public int DisplayOrder { get; set; }
}