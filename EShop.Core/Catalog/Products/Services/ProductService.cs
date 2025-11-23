using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data;

namespace EShop.Core.Catalog.Products.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;

    public ProductService(ApplicationDbContext db)
    {
        _db = db;
    }

    public ProductLazyContext CreateProductBatchContext(IEnumerable<Product> products, bool includeHidden = false)
    {
         return new ProductLazyContext(_db, products, includeHidden);
    }
}