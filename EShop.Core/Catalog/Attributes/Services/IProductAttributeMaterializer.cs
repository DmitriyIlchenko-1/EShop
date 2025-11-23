using EShop.Core.Catalog.Attributes.Domain;

namespace EShop.Core.Catalog.Attributes.Services;

public interface IProductAttributeMaterializer
{
    ProductVariantAttributeSelection CreateAttributeSelectionAsync(ProductVariantQuery query,
        IEnumerable<ProductVariantAttribute> attributes, int productId);

    IList<ProductVariantAttributeValue> MaterializeProductVariantAttributeValues(
        ProductVariantAttributeSelection selection,
        IEnumerable<ProductVariantAttribute> attributes);

    /// <summary>
    /// Finds and meterializes product variant attribute combination.
    /// </summary>
    /// <returns>Found <see cref="ProductVariantAttributeCombination"/></returns>
    Task<ProductVariantAttributeCombination> FindProductVariantAttributeCombinationAsync(int productId,
        ProductVariantAttributeSelection selection);
}