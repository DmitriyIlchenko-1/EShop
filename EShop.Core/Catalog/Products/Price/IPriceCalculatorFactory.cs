namespace EShop.Core.Catalog.Products.Price;

public interface IPriceCalculatorFactory
{
    ICollection<IPriceCalculator> Create(CalculatorPriceContext context);
}