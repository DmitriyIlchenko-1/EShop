using System.Collections.Immutable;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data;
using EShop.Core.Platform.Caching;
using EShop.Core.Platform.Common;
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
    private readonly PerformanceSettings _performanceSettings;
    private const string AttributeCombinationByIdHashCodeKey = "attributecombination:byproductid-{0}-{1}";

    private const string AttributeCombinationAvailabilityByIdCacheKey =
        "attributecombinationavailability:byproductid-{0}";


    public ProductAttributeMaterializer(ApplicationDbContext db, ICacheManager cache, IRequestCache requestCache,
        PerformanceSettings performanceSettings)
    {
        _db = db;
        _cache = cache;
        _requestCache = requestCache;
        _performanceSettings = performanceSettings;
    }

    public ProductVariantAttributeSelection CreateAttributeSelection(ProductVariantQuery query,
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


    public async Task<ProductVariantAttributeCombination> FindAttributeCombinationAsync(int productId,
        ProductVariantAttributeSelection selection)
    {
        if (productId == 0 || selection.IsNullOrEmpty())
        {
            return null;
        }

        int hashCode = selection.GetHashCode();
        var cacheKey = string.Format(AttributeCombinationByIdHashCodeKey, productId, hashCode);
        return await _requestCache.GetOrCreateAsync<ProductVariantAttributeCombination>(cacheKey,
            async () =>
            {
                return await _db
                    .ProductVariantAttributeCombinations.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ProductId == productId && x.HashCode == hashCode);
            });
    }


    public virtual bool TryGetPrefetchedCombination(int productId, ProductVariantAttributeSelection selection,
        out ProductVariantAttributeCombination combination)
    {
        if (productId == 0 || selection.IsNullOrEmpty())
        {
            combination = null;
            return false;
        }
    
        int hashCode = selection.GetHashCode();
        var cacheKey = string.Format(AttributeCombinationByIdHashCodeKey, productId, hashCode);
        _requestCache.TryGet<ProductVariantAttributeCombination>(cacheKey, out combination);
        return combination != null;
    }


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
        if (_performanceSettings.MaxUnavailableCombinations <= 0)
            return null;


        var cacheKey = string.Format(AttributeCombinationAvailabilityByIdCacheKey, product.Id);
        IDictionary<int, CombinationAvailabilityInfo> combinationAvailabilityInfos =
            await _cache.GetOrCreateAsync(cacheKey,
                async () =>
                {
                    // We query UNavailable combinations that are either inactive or whose stock quantity is zero or below.
                    var query = _db
                        .ProductVariantAttributeCombinations.AsNoTracking()
                        .Where(x => x.ProductId == product.Id)
                        .Where(x => !x.IsActive || x.StockQuantity <= 0)
                        .Select(x => new CombinationAvailabilityInfo()
                        {
                            ProductId = x.ProductId,
                            HashCode = x.HashCode,
                            IsActive = x.IsActive,
                            IsOutOfStock = x.StockQuantity <= 0,
                        });
                    var availableInfoCount = await query.CountAsync();
                    // Force to load if the product can only be ordered when you select a combination.
                    if (_performanceSettings.MaxUnavailableCombinations <= availableInfoCount)
                    {
                        var result = await query.ToListAsync();
                        // Each combination is stored in a dictionary where keys are combination hashcodes
                        return result.ToDictionary(x => x.HashCode, x => x);
                    }

                    return new Dictionary<int, CombinationAvailabilityInfo>();
                },
                new CacheEntryOptions(TimeSpan.FromHours(3)));


        // No UNavailable combinations - return null unless the product needs a combination to be ordered.
        if (combinationAvailabilityInfos.Count == 0 && !product.AttributeCombinationRequired)
            return null;
        var selection = new ProductVariantAttributeSelection();


        var selectedValuesMap = selectedVariantAttributeValues.ToMultiMap(x => x.ProductVariantAttributeId, x => x.Id);

        /* selectedValuesMap:
         * {
         *   Color: {
         *      Blue,
         *      ... (if multi-select)
         *      },
         *   Size: {
         *      Large,
         *      ... (if multi-select)
         *      },
         */

        foreach (var productVariantAttribute in productVariantAttributes.Where(x => x.IsListTypeAttribute()))
        {
            IEnumerable<int> chosenValueIds;
            // retrieve selected variant attribute values for the given variant attribute assigned to this product
            var selectedValues = selectedValuesMap.TryGetValue(productVariantAttribute.Id, out var valueIds)
                ? valueIds
                : null;

            if (productVariantAttribute.Id == currentVariantValue.ProductVariantAttributeId)
            {
                if (selectedValues != null && productVariantAttribute.IsMultipleChoices())
                {
                    chosenValueIds = selectedValues
                        .Append(currentVariantValue.Id)
                        .Distinct();
                }
                else
                {
                    // add the variant attribute value of the given attribute to the collection we'll then create a selection out of. 
                    chosenValueIds = new[] { currentVariantValue.Id };
                }
            }
            else
            {
                if (selectedValues == null)
                {
                    return null;
                }
                else
                {
                    chosenValueIds = selectedValues;
                }
            }


            // if the current variant value is from any other variant attribute that's not the current one, we just add all its choise value in here
            // (uually only one value, though, unless it's a multi choice)
            selection.AddAttribute(productVariantAttribute.Id, chosenValueIds);
        }

        var hashCode = selection.GetHashCode();
        if (combinationAvailabilityInfos.TryGetValue(hashCode, out var combination))
        {
            return combination;
        }

        // If a combination is required then:
        // If no UNavailable combinations were found (or this particular combination is not on that list), then we still have to prove it exists and if it does, it's considered Available.
        // If no UNavailable combinations were found because of the performance settings, we just go ahead and query the db to find the combination for the given selection and if it does exist,
        // it's considered Available even if it's stock value or 'IsActive' values are not valid.
        //TODO:  'considered Available even if it's stock value or 'IsActive' values are not valid ...' does it mean i can order this combination? 
        if (product.AttributeCombinationRequired && await FindAttributeCombinationAsync(product.Id, selection) == null)
        {
            return new() { IsActive = false };
        }
        // If 'AttributeCombinationRequired' is set to false, we just assume that if no UNavailable combinations were found (or this particular combination is not on that list),
        // the combination therefore exists,
        // but what it means in fact is that we just don't mark the given variant attribute values as 'inactive'. Why? see below.
        // We don't care if the combination exists because the attribute values that are going to be marked as 'Available' can be selected ANYWAY since we don't necessarily
        // need a combination to order the product.
        else
        {
            return null;
        }
    }

    public async Task<int> PrefetchProductVariantAttributeCombinationsAsync(
        IDictionary<int, IEnumerable<ProductVariantAttributeSelection>> selections)
    {
        Guard.NotNull(selections);
        if (!selections.Keys.Any() || !selections.Values.Any())
            return 0;
        var alreadyCollectedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        List<(int productId, string cacheKey, int hashCode)> combinationCacheInfos = new();

        foreach (var selectionPair in selections)
        {
            foreach (var selection in selectionPair.Value)
            {
                if (!selection.Attributes.Any())
                    continue;

                var productId = selectionPair.Key;
                var hashCode = selection.GetHashCode();
                var cacheKey = string.Format(AttributeCombinationByIdHashCodeKey, productId, hashCode);

                if (!alreadyCollectedKeys.Contains(cacheKey) || !_requestCache.Contains(cacheKey))
                {
                    //add the ones that haven't been loaded.
                    combinationCacheInfos.Add(new ValueTuple<int, string, int>(productId, cacheKey, hashCode));
                    alreadyCollectedKeys.Add(cacheKey);
                }   
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
}

public class CombinationAvailabilityInfo
{
    public int ProductId { get; set; }
    public int HashCode { get; set; }
    public bool IsActive { get; set; }
    public bool IsOutOfStock { get; set; }
}

public class AttributeVariantCountModel
{
    public int AttributeId { get; set; }
    public int ValueCount { get; set; }
}