namespace EShop.Core.Catalog.Products.Price;

public interface IPriceCalculatorFactory
{
    ICollection<IPriceCalculator> Create(PriceCalculatorContext context);
}