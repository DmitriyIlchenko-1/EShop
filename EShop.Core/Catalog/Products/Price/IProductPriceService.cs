using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Platform.Common;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Catalog.Products.Price;

public interface IProductPriceService
{
    Task<CalculatedPrice> CalculatePriceAsync(PriceCalculationContext context);   
    Task<(CalculatedPrice UnitPrice,CalculatedPrice Subtotal)> CalculateSubtotalAsync(PriceCalculationContext context);

}

public class DefaultProductPriceService : IProductPriceService
{
    private readonly IPriceCalculatorFactory _factory;
    private readonly IWorkContext _workContext;
    private readonly IProductAttributeMaterializer _productAttributeMaterializer;

    public DefaultProductPriceService(IPriceCalculatorFactory factory, IWorkContext workContext,
        IProductAttributeMaterializer productAttributeMaterializer)
    {
        _factory = factory;
        _workContext = workContext;
        _productAttributeMaterializer = productAttributeMaterializer;
    }

    public async Task<CalculatedPrice> CalculatePriceAsync(PriceCalculationContext context)
    {
        Guard.NotNull(context);
        var calculatorContext = await RunCalculatorsAsync(context);
        return await GetFinalPriceAsync(calculatorContext);
    }

    public async Task<(CalculatedPrice UnitPrice, CalculatedPrice Subtotal)> CalculateSubtotalAsync(PriceCalculationContext context)
    {
       var calculatorContext = await RunCalculatorsAsync(context);
       var price = await GetFinalPriceAsync(calculatorContext);
       if (context.Quantity <= 1)
       {
           
           return (price, price);
       }
       else
       {
           var subtotal = await GetFinalPriceAsync(calculatorContext, context.Quantity);
           return (price, subtotal);
       }
    }

    protected virtual async Task<CalculatorPriceContext> RunCalculatorsAsync(PriceCalculationContext context)
    {
        var product = context.Product;
        var ctx = new CalculatorPriceContext()
        {
            Product = product,
            CalculatedProductPrice = new CalculatedProductPrice()
            {
                RegularPrice = product.Price,
                FinalPrice = product.Price
            },
            BatchContext = context.BatchContext,
            User = _workContext.CurrentUser,
        };
        if (context.CartItem != null && context.CartItem.AttributeSelection != null)
        {
            var selectedCombination =
                await _productAttributeMaterializer.FindAttributeCombinationAsync(product.Id,
                    context.CartItem.AttributeSelection);
            ctx.Options.SelectedCombination = selectedCombination;
        }

        var calculators = _factory.Create(ctx);
        var dispatcher = new DefaultCalculatorDispatcher(calculators);
        await dispatcher.InvokeAsync(ctx);
        return ctx;
    }


    private async Task<CalculatedPrice> GetFinalPriceAsync(CalculatorPriceContext context, int subTotalQuantity = 1)
    {
        var product = context.Product;
        

        var calculatedPrice = new CalculatedPrice()
        {
            Product = product,
            FinalPrice = ConvertToMoney(context.CalculatedProductPrice.FinalPrice, true, context, subTotalQuantity)
                .Value,
            DiscountAmount = ConvertToMoney(context.CalculatedProductPrice.DiscountAmount, false, context, subTotalQuantity)
        };

        calculatedPrice.RegularPrice = ConvertToMoney(context.CalculatedProductPrice.RegularPrice, false, context, subTotalQuantity)
            .Value;

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

    protected virtual Money? ConvertToMoney(decimal? amount, bool isFinalPrice, CalculatorPriceContext ctx, int subtotalQuantity = 1)
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
        var money = new Money(amount.Value * subtotalQuantity);
        
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