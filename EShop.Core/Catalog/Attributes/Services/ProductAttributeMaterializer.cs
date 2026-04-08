using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Data;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;


namespace EShop.Core.Catalog.Attributes.Services;

public class ProductAttributeMaterializer : IProductAttributeMaterializer
{
    private readonly ApplicationDbContext _db;

    public ProductAttributeMaterializer(ApplicationDbContext db)
    {
        _db = db;
    }

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
        Guard.NotNull(selection);
        Guard.NotNull(attributes);

        var result = new List<ProductVariantAttributeValue>();

        if (!selection.IsNullOrEmpty())
        {
            var variantProductIDs = attributes
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.Id)
                .ToArray();

            var valueIDs = selection
                .Attributes
                .Where(x => variantProductIDs.Contains(x.Key)) // x.Key - attribute id.
                .Select(x => x.Value) // x.Value - the user selected value for that attribute.
                .Where(x => x > 0)
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

    public async Task<ICollection<ProductVariantAttributeCombination>> FindProductVariantAttributeCombinationsAsync(
        IDictionary<int, ProductVariantAttributeSelection> selections)
    {
        Guard.NotNull(selections);
        if (selections.Count == 0)
            return [];

        //TODO: Add caching. 
        var ids = selections
            .Select(x => x.Key)
            .ToArray();
        var hashCodes = selections
            .Where(x => !x.Value.IsNullOrEmpty())
            .Select(x => x.Value.GetHashCode())
            .ToArray();
        var combinations = await _db
            .ProductVariantAttributeCombinations.AsNoTracking()
            .Where(x => ids.Contains(x.ProductId) && hashCodes.Contains(x.HashCode))
            .ToListAsync();
        return combinations;
    }
}