using EShop.Core.Catalog.Products.Domain;

namespace EShop.Core.Catalog.Products.Services
{
    public interface IProductPricingService
    {
        CalculatedProductPrice CalculateProductPrice(Product product, int quantity = 1);

        CalculatedProductPrice CalculateProductPrice(decimal price, decimal? oldPrice, decimal? specialPrice, DateTime? specialPriceStart, DateTime? specialPriceEnd,int quantity = 1);
    }
}