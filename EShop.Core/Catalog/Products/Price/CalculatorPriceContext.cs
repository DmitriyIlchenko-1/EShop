using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Platform.Identity.Domain;

namespace EShop.Core.Catalog.Products.Price;

public class CalculatorPriceContext
{
    public Product Product { get; set; }
    public User User { get; set; }
    public ProductBatchContext BatchContext { get; set; }
    public CalculatorPriceOptions Options { get; set; } = new();
    public bool HasPriceRange { get; set; }
    public CalculatedProductPrice CalculatedProductPrice { get; set; } = new();
}

public class CalculatorPriceOptions
{
    public string PriceRangeFormat { get; set; }

    public bool ApplyDiscounts { get; set; } = true;
    
    public ProductVariantAttributeCombination SelectedCombination { get; set; }
}