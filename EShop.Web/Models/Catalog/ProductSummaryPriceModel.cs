namespace EShop.Web.Models.Catalog;

public class ProductSummaryPriceModel
{
    public decimal FinalPrice { get; set; }
    public decimal? OldPrice { get; set; }
    
    public int PercentOfSaving { get; set; }
}