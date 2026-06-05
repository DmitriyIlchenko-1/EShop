// using EShop.Core.Catalog.Products.Domain;
// using EShop.Core.Common.Services;
//
// namespace EShop.Core.Catalog.Products.Services;
//
// public class ProductPricingService : IProductPricingService
// {
//     private readonly ICurrencyService _currencyService;
//
//     public ProductPricingService(ICurrencyService currencyService)
//     {
//         _currencyService = currencyService;
//     }
//
//     public CalculatedProductPrice CalculateProductPrice(Product product, int quantity = 1)
//     {
//         CalculatedProductPrice calculatedProductPrice = CalculateProductPrice(product.Price,
//             product.OldPrice,
//             product.SpecialPrice,
//             product.SpecialPriceStartsUtc,
//             product.SpecialPriceEndsUtc);
//         return calculatedProductPrice;
//     }
//
//     
// }