using EShop.Core.Catalog.Products.Price;
using EShop.Core.Common.Services;

namespace EShop.Core.Catalog.Products.Domain;

public class CalculatedProductPrice
{
    public Discount? AppliedDiscount { get; set; }
    public decimal RegularPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
}

public readonly struct PriceSaving
{
    public bool HasSaving { get; init; }
    public Money SavingPrice { get; init; }
    public float SavingPercent { get; init; }
    public Money? SavingAmount { get; init; }
}

public class CalculatedPrice
{
    public Product Product { get; set; }
    public Money FinalPrice { get; set; }
    public Money? DiscountAmount { get; set; }
    public PriceSaving PriceSaving { get; set; }
    public Money RegularPrice { get; set; }
    public Discount? AppliedDiscount { get; set; }
}