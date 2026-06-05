using Autofac;

namespace EShop.Core.Catalog.Products.Price;

public class DefaultPriceCalculatorFactory : IPriceCalculatorFactory
{
    private readonly ILifetimeScope _scope;

    public DefaultPriceCalculatorFactory(ILifetimeScope scope)
    {
        _scope = scope;
    }

    public ICollection<IPriceCalculator> Create(CalculatorPriceContext context)
    {
        return _scope.Resolve<ICollection<IPriceCalculator>>();
    }
}