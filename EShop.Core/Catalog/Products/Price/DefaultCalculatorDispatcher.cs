using EShop.Infrastructure.Utilities;

namespace EShop.Core.Catalog.Products.Price;

public class DefaultCalculatorDispatcher
{
    private readonly IPriceCalculator[] _calculators;

    private static readonly CalculatorDelegate StartingPoint = _ => Task.CompletedTask;

    public DefaultCalculatorDispatcher(IEnumerable<IPriceCalculator> calculators)
    {
        Guard.NotNull(calculators);
        _calculators = calculators.OrderBy(x => x.Order).ToArray();
    }

    public async Task InvokeAsync(CalculatorPriceContext ctx)
    {
        Guard.NotNull(ctx);
        var delegates = CreateDelegates();

        CalculatorDelegate calculator = StartingPoint;
        for (var i = _calculators.Length - 1; i >= 0; i--)
        {
            calculator = delegates[i](calculator);
        }

        await calculator.Invoke(ctx);
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

public delegate Task CalculatorDelegate(CalculatorPriceContext ctx);