using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Common.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Attributes.Domain;

internal class ProductVariantAttributeCombinationMap : IEntityTypeConfiguration<ProductVariantAttributeCombination>
{
    public void Configure(EntityTypeBuilder<ProductVariantAttributeCombination> builder)
    {
        builder
            .HasOne(p => p.Product)
            .WithMany(p => p.ProductVariantAttributeCombinations)
            .HasForeignKey(p => p.ProductId);
        builder
            .HasOne(p => p.DeliveryTime)
            .WithMany()
            .HasForeignKey(p => p.DeliveryTimeId);
    }
}

public class ProductVariantAttributeCombination : BaseEntity
{
    public decimal? BasePriceAmount { get; set; }

    public decimal? BasePriceBaseAmount { get; set; }

    public DeliveryTime DeliveryTime { get; set; }

    public int? DeliveryTimeId { get; set; }

    public string Gtin { get; set; }

    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }

    public decimal? Width { get; set; }
    public decimal? Length { get; set; }

    public bool IsActive { get; set; }


    public string ManufacturerPartNumber { get; set; }

    public decimal? Price { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal? SpecialPrice { get; set; }
    public DateTime? SpecialPriceEnd { get; set; }

    public DateTime? SpecialPriceStarts { get; set; }
    public int HashCode { get; set; }
    public string RawAttributes { get; set; }

    public Product Product { get; set; }

    public int ProductId { get; set; }

    public int QuantityUnitId { get; set; }

    public string Sku { get; set; }

    public int StockQuantity { get; set; }
}