namespace EShop.Core.Catalog.Products.Services;

public interface IRecentlyViewedProductsService
{
    void AddProductToRecentlyViewedList(int productId);
}