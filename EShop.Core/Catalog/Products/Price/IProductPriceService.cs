using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Platform.Common;

namespace EShop.Core.Catalog.Products.Price;

public interface IProductPriceService
{
    Task<CalculatedPrice> CalculatePriceAsync(PriceCalculationContext context);
}

public class DefaultProductPriceService : IProductPriceService
{
    private readonly IPriceCalculatorFactory _factory;
    private readonly IWorkContext _workContext;

    public DefaultProductPriceService(IPriceCalculatorFactory factory, IWorkContext workContext)
    {
        _factory = factory;
        _workContext = workContext;
    }

    public async Task<CalculatedPrice> CalculatePriceAsync(PriceCalculationContext context)
    {
        var product = context.Product;
        var ctx = new CalculatorPriceContext()
        {
            Product = product,
            CalculatedProductPrice = new CalculatedProductPrice()
            {
                FinalPrice = product.Price
            },
            BatchContext = context.BatchContext,
            User = _workContext.CurrentUser,
        };
        var calculators = _factory.Create(ctx);
        var dispatcher = new DefaultCalculatorDispatcher(calculators);
        await dispatcher.InvokeAsync(ctx);
        return await GetFinalPriceAsync(ctx);
    }

    private async Task<CalculatedPrice> GetFinalPriceAsync(CalculatorPriceContext context)
    {
        var product = context.Product;
        
        var calculatedPrice = new CalculatedPrice()
        {
            Product = product,
            FinalPrice = ConvertToMoney(context.CalculatedProductPrice.FinalPrice, true, context).Value,
            DiscountAmount = ConvertToMoney(context.CalculatedProductPrice.DiscountAmount, false, context)
        };
        
        calculatedPrice.RegularPrice = ConvertToMoney(product.Price, false, context).Value;
        
        var savingPrice = calculatedPrice.RegularPrice;
        var hasSaving = savingPrice > 0 && calculatedPrice.FinalPrice < savingPrice;
        calculatedPrice.PriceSaving = new PriceSaving()
        {
            HasSaving = hasSaving,
            SavingPrice = savingPrice,
            SavingPercent = hasSaving ? (float)((savingPrice - calculatedPrice.FinalPrice) / savingPrice) * 100 : 0f,
            SavingAmount = hasSaving ? savingPrice - calculatedPrice.FinalPrice : null
        };
        return calculatedPrice;
    }

    protected virtual Money? ConvertToMoney(decimal? amount, bool isFinalPrice, CalculatorPriceContext ctx)
    {
        if (amount == null)
        {
            return null;
        }

        if (amount < 0)
        {
            amount = 0;
        }

        var options = ctx.Options;
        var money = new Money(amount.Value);
        if (isFinalPrice && ctx.HasPriceRange)
        {
            var finalPricePostFormat = money.PostFormat != null
                ? string.Format(options.PriceRangeFormat, money.PostFormat)
                : options.PriceRangeFormat;
            if (finalPricePostFormat != money.PostFormat)
            {
                money = money.WithPostFormat(finalPricePostFormat);
            }
        }

        return money;
    }
}