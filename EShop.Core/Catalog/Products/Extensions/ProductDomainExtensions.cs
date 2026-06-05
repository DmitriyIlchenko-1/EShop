using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Configuration;
using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Catalog.Products.Extensions;

public static class ProductDomainExtensions
{
    public static void MergeDataWithCombination(this Product product, ProductVariantAttributeCombination combination)
    {
        Guard.NotNull(product);
        if (combination == null)
        {
            return;
        }
        
        var values = product.MergedData;
        values?.Clear();
        
        if (values == null)
        {
            values = product.MergedData = new Dictionary<string, object>();
        }

        if (combination.Sku.HasValue())
            values.Add(nameof(Product.Sku), combination.Sku);

        values.Add(nameof(Product.Price), combination.Price);

        if (combination.Gtin.HasValue())
            values.Add(nameof(Product.Gtin), combination.Gtin);
        if (combination.Weight.HasValue)
            values.Add(nameof(Product.Weight), combination.Weight);
        if (combination.Height.HasValue)
            values.Add(nameof(Product.Height), combination.Height);
        if (combination.Length.HasValue)
            values.Add(nameof(Product.Length), combination.Length);
        if (combination.Width.HasValue)
            values.Add(nameof(Product.Width), combination.Width);
        if (combination.BasePriceAmount.HasValue)
            values.Add(nameof(Product.BasePriceAmount), combination.BasePriceAmount);
        if (combination.BasePriceBaseAmount.HasValue)
            values.Add(nameof(Product.BasePriceBaseAmount), combination.BasePriceBaseAmount);
        if (combination.QuantityUnitId.HasValue)
            values.Add(nameof(Product.QuantityUnitId), combination.QuantityUnitId);
        
        values.Add(nameof(Product.StockQuantity), combination.StockQuantity);
      

        // Delivery time 
        if (combination.DeliveryTimeId.HasValue && combination.DeliveryTimeId.Value > 0)
        {
            values.Add(nameof(Product.DeliveryTimeId), combination.DeliveryTimeId);
        }
    }

    public static bool IsNew(this Product product, CatalogSettings settings)
    {
        if (settings.LabelAsNewForMaxDays.HasValue && settings.LabelAsNewForMaxDays > 0)
        {
            return (DateTime.UtcNow - product.CreatedOnUtc).Days <= settings.LabelAsNewForMaxDays.Value;
        }

        return false;
    }
}