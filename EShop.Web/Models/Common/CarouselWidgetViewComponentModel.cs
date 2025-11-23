namespace EShop.Web.Models.Common;

public class CarouselWidgetViewComponentModel
{
    public long Id { get; set; }
    public int DataInterval { get; set; } = 6000;
    public ICollection<CarouselWidgetViewComponentItemModel> Items { get; set; }
}