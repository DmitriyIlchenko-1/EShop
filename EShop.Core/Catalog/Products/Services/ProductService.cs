using Autofac;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data;

namespace EShop.Core.Catalog.Products.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;
    private readonly IComponentContext _componentContext;

    public ProductService(ApplicationDbContext db, IComponentContext componentContext)
    {
        _db = db;
        _componentContext = componentContext;
    }

    public virtual ProductBatchContext CreateProductBatchContext(IEnumerable<Product> products, bool includeHidden = false)
    {
         return new ProductBatchContext(_db, products,_componentContext, includeHidden);
    }
}