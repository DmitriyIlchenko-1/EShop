using EShop.Core.Catalog.Attributes.Domain;

namespace EShop.Core.Catalog.Attributes.Services;

public interface IProductAttributeMaterializer
{
    /// <returns>Reads the selected attribute values from the query (each of the attribute values the user has selected e.g. Blue, 49mm etc and match the id of that attribute value with the attribute itself</returns>
    ProductVariantAttributeSelection CreateAttributeSelectionAsync(ProductVariantQuery query,
        IEnumerable<ProductVariantAttribute> attributes, int productId);

    /// <summary>
    /// Takes the attribute value ids extracted in <see cref="CreateAttributeSelectionAsync"/> and finds <see cref="ProductVariantAttributeValue"/> with the same id cuz this is the domain representation of the selected user attribute value.
    /// It matches all the ids with their domain entities and returns this set.
    /// </summary>
    /// <returns>A list of domain entities <see cref="ProductVariantAttributeValue"/> that match the selected attribute values in <see cref="selection"/></returns>
    IList<ProductVariantAttributeValue> MaterializeProductVariantAttributeValues(
        ProductVariantAttributeSelection selection,
        IEnumerable<ProductVariantAttribute> attributes);

    /// <summary>
    /// Finds and meterializes product variant attribute combination.
    /// </summary>
    /// <returns>Found <see cref="ProductVariantAttributeCombination"/></returns>
    Task<ICollection<ProductVariantAttributeCombination>> FindProductVariantAttributeCombinationsAsync(
        IDictionary<int, ProductVariantAttributeSelection> selections);
}