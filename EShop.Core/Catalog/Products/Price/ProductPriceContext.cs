using EShop.Core.Catalog.Products.Domain;

namespace EShop.Core.Catalog.Products.Price;

public class ProductPriceContext
{
    public Product Product { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public PriceDiscountContext PriceDiscountContext { get; set; } = new PriceDiscountContext();
    
}

public class PriceDiscountContext
{
    public int PercentOfSaving { get; set; }
    public Discount AppliedDiscount { get; set; }
}