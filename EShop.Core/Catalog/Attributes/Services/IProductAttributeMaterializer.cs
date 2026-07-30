using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products.Domain;

namespace EShop.Core.Catalog.Attributes.Services;

public interface IProductAttributeMaterializer
{
    /// <returns>Reads the selected attribute values from the query (each of the attribute values the user has selected e.g. Blue, 49mm etc and match the id of that attribute value with the attribute itself</returns>
    ProductVariantAttributeSelection CreateAttributeSelection(ProductVariantQuery query,
        IEnumerable<ProductVariantAttribute> attributes, int productId);

    /// <summary>
    /// Takes the attribute value ids extracted in <see cref="CreateAttributeSelection"/> and finds <see cref="ProductVariantAttributeValue"/> with the same id cuz this is the domain representation of the selected user attribute value.
    /// It matches all the ids with their domain entities and returns this set.
    /// </summary>
    /// <returns>A list of domain entities <see cref="ProductVariantAttributeValue"/> that match the selected attribute values in <see cref="selection"/></returns>
    IList<ProductVariantAttributeValue> MaterializeProductVariantAttributeValues(
        ProductVariantAttributeSelection selection,
        IEnumerable<ProductVariantAttribute> attributes);

    /// <summary>
    /// Finds and meterializes product variant attribute combination.
    /// </summary>
    /// <returns>Found <see cref="ProductVariantAttributeCombination"/>. Keys are product ids whose combinations are in Values</returns>
    Task<ProductVariantAttributeCombination> FindAttributeCombinationAsync(int productId,
        ProductVariantAttributeSelection selection);

    Task<int> PrefetchProductVariantAttributeCombinationsAsync(
        IDictionary<int, IEnumerable<ProductVariantAttributeSelection>> selections);
    bool TryGetPrefetchedCombination(int productId, ProductVariantAttributeSelection selection,
        out ProductVariantAttributeCombination combination);

    /// <summary>
    ///  This method accepts variant attributes related to a product.
    /// Then it takes every value from every product variant attribute and matches this value to every other value from every other product variant attribute.
    /// So essentially it matches this taken value with every other value from every other product variant attribute except from the product variant attribute the taken value is from.
    /// For example, if the current taken value is Red, then we match Red with Plastic from the material product variant attribute entity,
    /// and we also match Red with Metal, but we aren't interested in the rest of the colors at this moment,
    /// because a combination is formed with only one possible value from every product variant attribute.
    /// </summary>
    /// <param name="product"></param>
    /// <param name="productVariantAttributes">Variant attributes assigned to this product</param>
    /// <param name="selectedVariantAttributeValues">Variant attribute values that have been selected as the result of pre-selected values being chosen by a merchant or the overriden ones by the user.
    /// In other words, these are final 'checked/selected' values. </param>
    /// <param name="currentVariantValue"></param>
    /// <returns>The method returns null if the given combination is available and <see cref="CombinationAvailabilityInfo"/> if the combination is UNavailable  </returns>
    Task<CombinationAvailabilityInfo> IsCombinationAvailableAsync(Product product,
        IEnumerable<ProductVariantAttribute> productVariantAttributes,
        IEnumerable<ProductVariantAttributeValue> selectedVariantAttributeValues,
        ProductVariantAttributeValue currentVariantValue);

     
    
}