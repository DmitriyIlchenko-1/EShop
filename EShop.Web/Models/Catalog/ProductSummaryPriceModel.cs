namespace EShop.Web.Models.Catalog;

public class ProductSummaryPriceModel
{
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int PercentOfSaving { get; set; }
}