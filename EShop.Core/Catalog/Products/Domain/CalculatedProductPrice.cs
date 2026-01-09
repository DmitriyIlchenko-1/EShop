using EShop.Core.Common.Services;

namespace EShop.Core.Catalog.Products.Domain;

public class CalculatedProductPrice
{ 
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int PercentOfSaving { get; set; }
    public string PriceString { get; set; }
    public string OldPriceString { get; set; }
}