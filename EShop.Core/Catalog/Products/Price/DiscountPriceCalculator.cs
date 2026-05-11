using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Services;

namespace EShop.Core.Catalog.Products.Price;

public class DiscountPriceCalculator : IPriceCalculator
{
    private readonly IDiscountService _discountService;
   
    public DiscountPriceCalculator(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    public int Order { get; }
    public async Task CalculateAsync(ProductPriceContext context, CalculatorDelegate next)
    {
        var product = context.Product;
        var discounts = await _discountService.GetAppliedDiscountsByProductIdAsync(context.Product.Id, false);
        (Discount appliedDiscount, decimal finalPrice) = ChooseBestCandidate(product, discounts);
        context.PriceDiscountContext.AppliedDiscount = appliedDiscount;
        context.FinalPrice = finalPrice;
    }

    private static (Discount, decimal) ChooseBestCandidate(Product product, IEnumerable<Discount> discounts)
    {
        decimal finalPrice = 0m;
        Discount topMatch = null;
        foreach (var discount in discounts)
        {
            decimal price;
            if (discount.UsePercentage)
            {
                price = product.Price - ((discount.DiscountAmount / 100m) * product.Price);
            }
            else
            {
                price = product.Price - discount.DiscountAmount;
            }
            
            topMatch = finalPrice < price ? discount : topMatch;
        }

        return (topMatch, finalPrice);
    }
}