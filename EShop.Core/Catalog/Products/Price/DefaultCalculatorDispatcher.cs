namespace EShop.Core.Catalog.Products.Price;

public class DefaultCalculatorDispatcher
{
    private readonly IPriceCalculator[] _calculators;
    private readonly PriceCalculatorContext _context;

    private static readonly CalculatorDelegate DefaultCalculator = (context) =>
    {
        if (context.FinalPrice == 0)
        {
            context.FinalPrice = context.Product.Price;
        }
        
        return Task.CompletedTask;
    };

    public DefaultCalculatorDispatcher(IEnumerable<IPriceCalculator> calculators, PriceCalculatorContext context)
    {
        _calculators = calculators.OrderBy(x => x.Order).ToArray();
        _context = context;
    }

    public async Task<ProductPriceContext> InvokeAsync()
    {
        var productPriceContext = new ProductPriceContext()
        {
            Product = _context.Product,
        };
        
        var delegates = CreateDelegates();

        CalculatorDelegate calculator = DefaultCalculator;
        for (var i = _calculators.Length - 1; i >= 0; i--)
        {
            calculator = delegates[i](calculator);
        }

        await calculator.Invoke(productPriceContext);

        return productPriceContext;
    }

    private List<Func<CalculatorDelegate, CalculatorDelegate>> CreateDelegates()
    {
        List<Func<CalculatorDelegate, CalculatorDelegate>> delegates = new();
        
        foreach (var calculator1 in _calculators)
        {
            delegates.Add(next =>
            { 
                return async context =>
                {
                    await calculator1.CalculateAsync(context, next);
                };
            });
        }

        return delegates;
    }

    
}

public delegate Task CalculatorDelegate(ProductPriceContext ctx);