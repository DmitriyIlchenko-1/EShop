using System.Collections.Immutable;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data;
using EShop.Core.Platform.Caching;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Collections;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;


namespace EShop.Core.Catalog.Attributes.Services;

public class ProductAttributeMaterializer : IProductAttributeMaterializer
{
    private readonly ApplicationDbContext _db;
    private readonly ICacheManager _cache;
    private readonly IRequestCache _requestCache;
    private const string AttributeCombinationByIdHashCodeKey = "attributecombination:byproductid-{0}-{1}";

    private const string AttributeCombinationAvailabilityByIdCacheKey =
        "attributecombinationavailability:byproductid-{0}";

    private const string AttributeValueCountKey = "variantattributevalue:count:{0}";


    public ProductAttributeMaterializer(ApplicationDbContext db, ICacheManager cache, IRequestCache requestCache)
    {
        _db = db;
        _cache = cache;
        _requestCache = requestCache;
    }

    public ProductVariantAttributeSelection CreateAttributeSelectionAsync(ProductVariantQuery query,
        IEnumerable<ProductVariantAttribute> attributes, int productId)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(attributes, nameof(attributes));

        ProductVariantAttributeSelection selection = new ProductVariantAttributeSelection();
        foreach (var attribute in attributes)
        {
            // 3 2 10
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

            if (valueId != 0)
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
                .SelectMany(x => x.Value) // x.Value - the user selected value/s for that attribute.
                .Select(x => x)
                .Where(x => x != 0)
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


    public async Task<int> PrefetchProductVariantAttributeCombinationsAsync(
        IDictionary<int, ProductVariantAttributeSelection> selections)
    {
        Guard.NotNull(selections);
        if (!selections.Any())
            return 0;
        var alreadyCollectedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        List<(int productId, string cacheKey, int hashCode)> combinationCacheInfos = new();

        foreach (var selectionPair in selections)
        {
            if (!selectionPair.Value.Attributes.Any())
                continue;

            var productId = selectionPair.Key;
            var selection = selectionPair.Value;
            var hashCode = selection.GetHashCode();
            var cacheKey = string.Format(AttributeCombinationByIdHashCodeKey, productId, hashCode);

            if (!alreadyCollectedKeys.Contains(cacheKey) || !_requestCache.Contains(cacheKey))
            {
                //add the ones that haven't been loaded.
                combinationCacheInfos.Add(new ValueTuple<int, string, int>(productId, cacheKey, hashCode));
                alreadyCollectedKeys.Add(cacheKey);
            }
        }

        //Contains(x.ProductId)
        var allProductIds = combinationCacheInfos
            .Select(x => x.productId)
            .Where(x => x != 0)
            .Distinct()
            .ToArray();

        //Contains(x.HashCode)
        var allHashCodes = combinationCacheInfos
            .Select(x => x.hashCode)
            .Where(x => x != 0)
            .Distinct()
            .ToArray();


        // Load all values in one go.
        var combinations = await _db
            .ProductVariantAttributeCombinations.AsNoTracking()
            .Where(x => allProductIds.Contains(x.ProductId) && allHashCodes.Contains(x.HashCode))
            .ToListAsync();
        var combinationMap = combinations.ToDictionary(x => x.HashCode);

        foreach (var info in combinationCacheInfos)
        {
            if (combinationMap.TryGetValue(info.hashCode, out var combination))
            {
                _requestCache.Put(info.cacheKey, combination);
            }
        }

        return combinationCacheInfos.Count;
    }

    public async Task<ProductVariantAttributeCombination> FindAttributeCombinationAsync(int productId,
        ProductVariantAttributeSelection selection)
    {
        if (productId == 0 || selection.IsNullOrEmpty())
        {
            return null;
        }
        
        int hashCode = selection.GetHashCode();
        return await _db
            .ProductVariantAttributeCombinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.HashCode == hashCode);
    }

    public virtual ProductVariantAttributeCombination GetPrefetchedCombinationOrDefault(int productId,
        ProductVariantAttributeSelection selection)
    {
        if (productId == 0 || selection.IsNullOrEmpty())
        {
            return null;
        }

        int hashCode = selection.GetHashCode();
        var cacheKey = string.Format(AttributeCombinationByIdHashCodeKey, productId, hashCode);
        _requestCache.TryGet<ProductVariantAttributeCombination>(cacheKey, out var result);
        return result;
    }


    /// <summary>
    /// This method accepts variant attributes related to a product.
    /// Then it takes every value from every product variant attribute and matches this value to every other value from every other product variant attribute.
    /// So essentially it matches this taken value with every other value from every other product variant attribute except from the product variant attribute the taken value is from.
    /// For example, if the current taken value is Red, then we match Red with Plastic from the material product variant attribute entity,
    /// and we also match Red with Metal, but we aren't interested in the rest of the colors at this moment,
    /// because a combination is formed with only one possible value from every product variant attribute.
    /// </summary>
    /// <param name="product"></param>
    /// <param name="attributes"></param>
    /// <param name="variantAttributes"></param>
    /// <returns></returns>
    public virtual async Task<CombinationAvailabilityInfo> IsCombinationAvailableAsync(Product product,
        IEnumerable<ProductVariantAttribute> productVariantAttributes,
        IEnumerable<ProductVariantAttributeValue> selectedVariantAttributeValues,
        ProductVariantAttributeValue currentVariantValue)
    {
        Guard.NotNull(product);
        Guard.NotNull(productVariantAttributes);
        Guard.NotNull(selectedVariantAttributeValues);
        Guard.NotNull(currentVariantValue);
        if (!productVariantAttributes.Any() || !selectedVariantAttributeValues.Any())
            return null;

        var cacheKey = string.Format(AttributeCombinationAvailabilityByIdCacheKey, product.Id);
        var unavailableComb = await _cache.GetOrCreateAsync(cacheKey,
            async () =>
            {
                return await _db
                    .ProductVariantAttributeCombinations.AsNoTracking()
                    .Where(x => x.ProductId == product.Id)
                    .Where(x => !x.IsActive || x.StockQuantity <= 0)
                    .Select(x => new CombinationAvailabilityInfo()
                    {
                        HashCode = x.HashCode,
                        IsActive = x.IsActive,
                        IsOutOfStock = x.StockQuantity <= 0,
                    })
                    .ToDictionaryAsync(x => x.HashCode, x => x);
            },
            new CacheEntryOptions() { AbsoluteExpiration = TimeSpan.FromSeconds(60) });

        if (unavailableComb.Count == 0 && !product.AttributeCombinationRequired)
            return null;
        var selection = new ProductVariantAttributeSelection();
        var selectedAttributesMap =
            selectedVariantAttributeValues.ToMultiMap(x => x.ProductVariantAttributeId, x => x.Id);
        foreach (var productVariantAttribute in productVariantAttributes.Where(x => x.IsListTypeAttribute()))
        {
            var selectedValues = selectedAttributesMap.TryGetValue(productVariantAttribute.Id, out var valueIds)
                ? valueIds
                : [];
            if (productVariantAttribute.Id == currentVariantValue.ProductVariantAttributeId)
            {
                if (productVariantAttribute.IsMultipleChoices())
                {
                    selectedValues = selectedValues
                        .Append(currentVariantValue.Id)
                        .Distinct();
                }
                else
                {
                    selectedValues = new[] { currentVariantValue.Id };
                }
            }

            selection.AddAttribute(productVariantAttribute.Id, selectedValues);
        }

        var hashCode = selection.GetHashCode();
        if (unavailableComb.TryGetValue(hashCode, out var combination))
        {
            return combination;
        }

        if (product.AttributeCombinationRequired
            && await FindAttributeCombinationAsync(product.Id, selection) == null)
        {
            return new()
            {
                IsActive = false
            };
        }
        else
        {
            return null;
        }
    }

    public virtual async Task<IDictionary<int, int>> GetEssentialVariantAttributeValueCountsAsync(
        bool isRequiredOnly = false)
    {
        var cacheKey = string.Format(AttributeValueCountKey, isRequiredOnly);
        return await _cache.GetOrCreateAsync(cacheKey,
            async () =>
            {
                var q1 = _db
                    .ProductVariantAttributes.Where(x => !isRequiredOnly || x.IsRequired
                        && x.ProductVariantAttributeValues.Any(x => x.IsEssential))
                    .AsNoTracking();
                return await q1
                    .Select(x => new AttributeVariantCountModel()
                    {
                        AttributeId = x.Id,
                        ValueCount = x.ProductVariantAttributeValues.Count
                    })
                    .ToDictionaryAsync(x => x.AttributeId, x => x.ValueCount);
            },
            new CacheEntryOptions() { AbsoluteExpiration = TimeSpan.FromSeconds(60) });
    }
}

public class CombinationAvailabilityInfo
{
    public int HashCode { get; set; }
    public bool IsActive { get; set; }
    public bool IsOutOfStock { get; set; }
}

public class AttributeVariantCountModel
{
    public int AttributeId { get; set; }
    public int ValueCount { get; set; }
}