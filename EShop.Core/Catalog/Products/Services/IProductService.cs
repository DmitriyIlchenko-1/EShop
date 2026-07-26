using EShop.Core.Catalog.Products.Domain;

namespace EShop.Core.Catalog.Products.Services;

public interface IProductService
{
    ProductBatchContext CreateProductBatchContext(
        IEnumerable<Product> products, bool includeHidden = false);

    Task AdjustProductInventoryAsync(Product product, int newQuantity, string rawAttributes = null);
}