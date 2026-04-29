using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Catalog.Attributes.Extensions;

public static class ProductAttributeMaterializerExtensions
{
    public static async Task<int> GetEssentialVariantAttributeValueCountAsync(this IProductAttributeMaterializer materializer, int variantAttribute,
        bool isRequiredOnly = false)
    {
        Guard.NotNull(materializer);
        if (variantAttribute == 0)
            return 0;
        var counts = await materializer.GetEssentialVariantAttributeValueCountsAsync(isRequiredOnly);
        return counts.TryGetValue(variantAttribute, out var count) ? count : 0;

    }
    
    
}