namespace EShop.Web.Common.Models.Choices;

public abstract class ChoiceItemModel : BaseModel
{
    public string Name { get; set; }
    public string Title { get; set; }
    public string Alias { get; set; }
    public string Color { get; set; }
    public string PriceAdjustment { get; set; }
    public decimal PriceAdjustmentValue { get; set; }
    public int QuantityInfo { get; set; }
    public bool IsPreSelected { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsUnavailable { get; set; }
    public int DisplayOrder { get; set; }
}