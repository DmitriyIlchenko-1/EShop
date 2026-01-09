using EShop.Core.Catalog.Products.Domain;

namespace EShop.Core.Catalog.Products.Services;

public interface IRecentlyViewedProductsService
{
    void AddProductToRecentlyViewedList(int productId);
    Task<IList<Product>> GetRecentlyViewedProducts(int count, int? productToSkipId);
}