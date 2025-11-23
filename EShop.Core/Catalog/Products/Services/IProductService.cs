using EShop.Core.Catalog.Products.Domain;

namespace EShop.Core.Catalog.Products.Services;

public interface IProductService
{
    ProductLazyContext CreateProductBatchContext(
        IEnumerable<Product> products, bool includeHidden = false);
}