using Autofac;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Catalog.Products.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;
    private readonly IComponentContext _componentContext;
    private readonly IProductAttributeMaterializer _attributeMaterializer;

    public ProductService(ApplicationDbContext db, IComponentContext componentContext,
        IProductAttributeMaterializer attributeMaterializer)
    {
        _db = db;
        _componentContext = componentContext;
        _attributeMaterializer = attributeMaterializer;
    }

    public virtual ProductBatchContext CreateProductBatchContext(IEnumerable<Product> products,
        bool includeHidden = false)
    {
        return new ProductBatchContext(_db, products, _componentContext, includeHidden);
    }

    public virtual async Task AdjustProductInventoryAsync(Product product, int adjustedQuantity,
        string rawAttributes = null)
    {
        //TODO: add a lock or something because right now if two requests come in for the same product, the quantity might drop below zero when we do NOT want that. We want to may be discard the later request if that happens.
        Guard.NotNull(product);
        if (adjustedQuantity == 0)
        {
            return;
        }

        if (rawAttributes is not null)
        {
            var selection = new ProductVariantAttributeSelection(rawAttributes);
            var combination = await _attributeMaterializer.FindAttributeCombinationAsync(product.Id, selection);
            if (combination is not null)
            {
                combination.StockQuantity += adjustedQuantity;
            }
        }
        else
        {
            product.StockQuantity += adjustedQuantity;
        }

        await _db.SaveChangesAsync();
    }
}