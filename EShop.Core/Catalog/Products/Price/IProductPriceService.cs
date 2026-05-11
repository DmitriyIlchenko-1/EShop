namespace EShop.Core.Catalog.Products.Price;

public interface IProductPriceService
{
    Task<ProductPriceContext> CalculatePriceAsync(PriceCalculatorContext context);
}

public class DefaultProductPriceService : IProductPriceService
{
    private readonly IPriceCalculatorFactory _factory;

    public DefaultProductPriceService(IPriceCalculatorFactory factory)
    {
        _factory = factory;
    }

    public async Task<ProductPriceContext> CalculatePriceAsync(PriceCalculatorContext context)
    {
        var calculators = _factory.Create(context);
        var dispatcher = new DefaultCalculatorDispatcher(calculators, context);
        return await dispatcher.InvokeAsync();
    }
}