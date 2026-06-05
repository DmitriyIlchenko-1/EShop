namespace EShop.Core.Catalog.Products.Price;

public interface IPriceCalculator
{
    int Order { get; }
    Task CalculateAsync(CalculatorPriceContext context, CalculatorDelegate next);
}