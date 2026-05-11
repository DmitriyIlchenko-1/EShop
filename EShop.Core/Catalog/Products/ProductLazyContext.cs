using Autofac;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Content.Media.Domain;
using EShop.Core.Data;
using EShop.Infrastructure.Collections;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
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
    private readonly bool _includeHidden;
    private readonly ApplicationDbContext _db;
    protected readonly IComponentContext _componentContext;

    private LazyMultimap<ProductVariantAttribute> _attributes;
    private LazyMultimap<ProductMedia> _productMedia;
    private LazyMultimap<ProductVariantAttributeCombination> _attributeCombinations;
    private LazyMultimap<ProductLink> _relatedProducts;
    private LazyMultimap<ProductSpecificationAttribute> _specifications { get; set; }

    public ProductLazyContext(ApplicationDbContext db, IEnumerable<Product> products,
        IComponentContext componentContext, bool includeHidden = false)
    {
        Guard.NotNull(db);
        Guard.NotNull(componentContext);
        _db = db;
        _componentContext = componentContext;
        _includeHidden = includeHidden;

        if (products != null)
        {
            _productIds.AddRange(products.Select(p => p.Id));
        }
    }

    protected IBrandService _brandService;

    internal IBrandService BrandService
    {
        get => _brandService ??= _componentContext.Resolve<IBrandService>();
        set => _brandService = value;
    }


    public LazyMultimap<ProductVariantAttribute> Attributes
        => _attributes ??= new LazyMultimap<ProductVariantAttribute>(LoadVariantAttributes, _productIds);

   

    public LazyMultimap<ProductSpecificationAttribute> ProductSpecification
        => _specifications ??=
            new LazyMultimap<ProductSpecificationAttribute>(LoadSpecificationAttributes, _productIds);

    public LazyMultimap<ProductLink> RelatedProducts
        => _relatedProducts ??= new LazyMultimap<ProductLink>(LoadRelatedProducts, _productIds);
    
    public LazyMultimap<ProductMedia> ProductMedia
    => _productMedia ??= new LazyMultimap<ProductMedia>(LoadProductMedia, _productIds);

    protected virtual async Task<MultiMap<int, ProductMedia>> LoadProductMedia(int[] ids)
    {
        return (await _db
            .ProductMedias.AsNoTracking()
            .Include(x => x.MediaFile)
            .Where(x => ids.Contains(x.ProductId))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.ProductId)
            .ToListAsync()).ToMultiMap(x => x.ProductId, x => x);
    }

   

    private async Task<MultiMap<int, ProductVariantAttribute>> LoadVariantAttributes(int[] ids)
    {
        var attributes = await BuildVariantAttributeQuery(_db, ids).ToListAsync();
        return attributes.ToMultiMap(x => x.ProductId, x => x);
    }
    
    private static IQueryable<ProductVariantAttribute> BuildVariantAttributeQuery(ApplicationDbContext db, int[] ids)
    {
        return db
            .ProductVariantAttributes
            .AsNoTracking()
            .Include(x => x.ProductAttribute)
            .Include(x => x.ProductVariantAttributeValues)
            .Where(x => ids.Contains(x.ProductId))
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.DisplayOrder);
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
        var specifications = await BuildSpecificationQuery(_db, ids)
            .ToListAsync();
        return specifications.ToMultiMap(x => x.ProductId, x => x);
    }

    private static IQueryable<ProductSpecificationAttribute> BuildSpecificationQuery(ApplicationDbContext db,
        int[] ids)
    {
        return db
            .ProductSpecificationAttributes
            .AsNoTracking()
            .Where(x => ids.Contains(x.ProductId))
            .Include(x => x.SpecificationAttributeOption)
            .ThenInclude(x => x.SpecificationAttribute)
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.SpecificationAttributeOption.SpecificationAttribute.DisplayOrder)
            .ThenBy(x => x.SpecificationAttributeOption.SpecificationAttribute.Name);
    }

    public void Clear()
    {
        _productIds.Clear();
        _specifications?.Clear();
        _relatedProducts?.Clear();
        _attributes?.Clear();
        _attributeCombinations?.Clear();
    }
}