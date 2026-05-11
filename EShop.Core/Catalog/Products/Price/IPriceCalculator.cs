namespace EShop.Core.Catalog.Products.Price;

public interface IPriceCalculator
{
    int Order { get; }
    Task CalculateAsync(ProductPriceContext context, CalculatorDelegate next);
}