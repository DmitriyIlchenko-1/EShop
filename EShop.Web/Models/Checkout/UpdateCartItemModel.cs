using EShop.Web.Common.Models;

namespace EShop.Web.Models.Checkout;

public class UpdateCartItemModel : BaseModel
{
    public int CartItemId { get; set; }
    public int NewQuantity { get; set; }
}