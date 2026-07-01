using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Catalog.Products.Price;

public class CombinationPriceCalculator : IPriceCalculator
{
    private readonly ApplicationDbContext _db;

    public CombinationPriceCalculator(ApplicationDbContext db)
    {
        _db = db;
    }

    public int Order => CalculatorOrder.DetermineStartingPrice;

    public async Task CalculateAsync(CalculatorPriceContext context, CalculatorDelegate next)
    {
        if (context.Options.SelectedCombination != null)
        {
            context.CalculatedProductPrice.RegularPrice = context.Options.SelectedCombination.Price;
            context.CalculatedProductPrice.FinalPrice = context.Options.SelectedCombination.Price;

        }

        await next(context);
    }
}