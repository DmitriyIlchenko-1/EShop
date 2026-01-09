using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Common.Services;

namespace EShop.Core.Catalog.Products.Services;

public class ProductPricingService : IProductPricingService
{
    private readonly ICurrencyService _currencyService;

    public ProductPricingService(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    public CalculatedProductPrice CalculateProductPrice(Product product, int quantity = 1)
    {
        CalculatedProductPrice calculatedProductPrice = CalculateProductPrice(product.Price,
            product.OldPrice,
            product.SpecialPrice,
            product.SpecialPriceStartsUtc,
            product.SpecialPriceEndsUtc);
        return calculatedProductPrice;
    }

    public CalculatedProductPrice CalculateProductPrice(decimal price, decimal? oldPrice, decimal? specialPrice,
        DateTime? specialPriceStart, DateTime? specialPriceEnd, int quantity = 1)
    {
        int percentOfSaving = 0;
        decimal calculatedPrice = price;
        if (specialPrice.HasValue && specialPriceStart < DateTime.UtcNow && DateTimeOffset.UtcNow < specialPriceEnd)
        {
            calculatedPrice = specialPrice.Value;
            if (!oldPrice.HasValue || oldPrice < price)
            {
                oldPrice = price;
            }
        }

        if (oldPrice.HasValue && oldPrice.Value > 0 && oldPrice > calculatedPrice)
        {
            percentOfSaving = (int)(100 - Math.Ceiling((calculatedPrice / oldPrice.Value) * 100));
        }

        return new CalculatedProductPrice
        {
            Price = calculatedPrice,
            OldPrice = oldPrice,
            PercentOfSaving = percentOfSaving,
            PriceString = _currencyService.FormatCurrency(calculatedPrice),
            OldPriceString = oldPrice.HasValue ? _currencyService.FormatCurrency(oldPrice.Value) : string.Empty
        };
    }
}