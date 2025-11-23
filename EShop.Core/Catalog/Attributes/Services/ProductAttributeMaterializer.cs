using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Data;
using Microsoft.EntityFrameworkCore;
 

namespace EShop.Core.Catalog.Attributes.Services;

public class ProductAttributeMaterializer : IProductAttributeMaterializer
{
    private readonly ApplicationDbContext _db;

    public ProductAttributeMaterializer(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <returns>Returns a read-only dictionary containing the IDs of the selected variant attributes and the IDs of their values selected</returns>
    public ProductVariantAttributeSelection CreateAttributeSelectionAsync(ProductVariantQuery query,
        IEnumerable<ProductVariantAttribute> attributes, int productId)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(attributes, nameof(attributes));

        ProductVariantAttributeSelection selection = new ProductVariantAttributeSelection();
        foreach (var attribute in attributes)
        {
            var selectedVariant = query.Variants.FirstOrDefault(x =>
                x.ProductId == productId &&
                x.AttributeId == attribute.ProductAttributeId &&
                x.VariantAttributeId == attribute.Id
            );

            if (selectedVariant == null)
            {
                continue;
            }

            int.TryParse(selectedVariant.Value, out int valueId);

            if (valueId > 1)
            {
                selection.AddAttribute(attribute.Id, valueId);
            }
        }

        return selection;
    }

    public IList<ProductVariantAttributeValue> MaterializeProductVariantAttributeValues(
        ProductVariantAttributeSelection selection,
        IEnumerable<ProductVariantAttribute> attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes, nameof(attributes));

        var result = new List<ProductVariantAttributeValue>();

        if (!selection.IsNullOrEmpty())
        {
            var variantProductIDs = attributes
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.Id)
                .ToArray();

            var valueIDs = selection
                .Attributes
                .Where(x => variantProductIDs.Contains(x.Key))
                .Select(x => x.Value)
                .Where(x => x > 0) // <----- @
                .ToArray();

            foreach (var valueId in valueIDs)
            {
                foreach (var attribute in attributes)
                {
                    var attributeValue = attribute.ProductVariantAttributeValues.FirstOrDefault(x => x.Id == valueId);
                    if (attributeValue != null)
                    {
                        result.Add(attributeValue);
                    }
                }
            }
        }

        return result;
    }

    public async Task<ProductVariantAttributeCombination> FindProductVariantAttributeCombinationAsync(int productId,
        ProductVariantAttributeSelection selection)
    {
        if (productId == 0 || selection.IsNullOrEmpty())
        {
            return null;
        }

        //TODO: Add caching. 
        int hashCode = selection.GetHashCode();
        var combination = await _db
            .ProductVariantAttributeCombinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.HashCode == hashCode);
        return combination;
    }
}