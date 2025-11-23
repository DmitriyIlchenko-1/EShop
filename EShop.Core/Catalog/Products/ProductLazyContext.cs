using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data;
using EShop.Infrastructure.Collections;
using EShop.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Catalog.Products;

/// <summary>
/// This context is used to lazy load some of the product's properties.
/// Having this type allows us to change the lazy load strategy separately as well as freeing the domain from having to deal with lazy loading issues.
/// When we create an instance of this type, it contains just its products' ids.
/// We've got groups of fields that, if accessed, will load all the related data and return it.
/// Subsequent calls will then not trigger calls to the database.
/// </summary>
public class ProductLazyContext
{
    private readonly List<int> _productIds = [];
    private readonly ApplicationDbContext _db;

    private LazyMultimap<ProductVariantAttribute> _attributes;
    private LazyMultimap<ProductVariantAttributeCombination> _attributeCombinations;
    private LazyMultimap<ProductLink> _relatedProducts;
    private LazyMultimap<ProductSpecificationAttribute> _specifications { get; set; }

    public ProductLazyContext(ApplicationDbContext db, IEnumerable<Product> products, bool includeHidden = false)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
        if (products != null)
        {
            _productIds.AddRange(products.Select(p => p.Id));
        }
    }


    public LazyMultimap<ProductVariantAttribute> Attributes
        => _attributes ??= new LazyMultimap<ProductVariantAttribute>(LoadVariantAttributes, _productIds);

    public LazyMultimap<ProductVariantAttributeCombination> AttributeCombinations
        => _attributeCombinations ??=
            new LazyMultimap<ProductVariantAttributeCombination>(LoadAttributeCombinations, _productIds);

    public LazyMultimap<ProductSpecificationAttribute> ProductSpecification
        => _specifications ??=
            new LazyMultimap<ProductSpecificationAttribute>(LoadSpecificationAttributes, _productIds);

    public LazyMultimap<ProductLink> RelatedProducts
        => _relatedProducts ??= new LazyMultimap<ProductLink>(LoadRelatedProducts, _productIds);

    private async Task<MultiMap<int, ProductVariantAttribute>> LoadVariantAttributes(int[] ids)
    {
        var attributes = await _db
            .ProductVariantAttributes
            .AsNoTracking()
            .Include(x => x.ProductAttribute)
            .Include(x => x.ProductVariantAttributeValues)
            .Where(x => ids.Contains(x.Id))
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.DisplayOrder)
            .ToListAsync();

        return attributes.ToMultiMap(x => x.ProductId, x => x);
    }

    private async Task<MultiMap<int, ProductVariantAttributeCombination>> LoadAttributeCombinations(int[] ids)
    {
        var combinations = await _db
            .ProductVariantAttributeCombinations
            .AsNoTracking()
            .Where(x => ids.Contains(x.ProductId))
            .OrderBy(x => x.ProductId)
            .ToListAsync();
        return combinations.ToMultiMap(x => x.ProductId, x => x);
    }

    private async Task<MultiMap<int, ProductLink>> LoadRelatedProducts(int[] ids)
    {
        var relatedProducts = await _db
            .ProductLinks
            .AsNoTracking()
            .Where(x => ids.Contains(x.ProductId))
            .Include(x => x.LinkedProduct)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.ProductId)
            .ToListAsync();
 

        return relatedProducts.ToMultiMap(x => x.ProductId, x => x);
    }

    private async Task<MultiMap<int, ProductSpecificationAttribute>> LoadSpecificationAttributes(int[] ids)
    {
        //TODO: cache the results
        var specifications = await BuildSpecificationQuery(_db, ids, null)
            .ToListAsync();
        return specifications.ToMultiMap(x => x.ProductId, x => x);
    }

    private static IQueryable<ProductSpecificationAttribute> BuildSpecificationQuery(Data.ApplicationDbContext db,
        int[] ids,
        bool? essentialAttributes)
    {
        return db
            .ProductSpecificationAttributes
            .AsNoTracking()
            .Where(x => ids.Contains(x.ProductId) &&
                        x.SpecificationAttributeOption.SpecificationAttribute.IsEssential ||
                        x.SpecificationAttributeOption.SpecificationAttribute.ShowOnProductPage)
            .Include(x => x.SpecificationAttributeOption)
            .ThenInclude(x => x.SpecificationAttribute)
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.SpecificationAttributeOption.SpecificationAttribute.DisplayOrder)
            .ThenBy(x => x.SpecificationAttributeOption.SpecificationAttribute.Name);
    }
}