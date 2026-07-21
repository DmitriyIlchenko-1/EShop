
using EShop.Web.Common.Models;

namespace EShop.Web.Models.Checkout;

public class CheckoutModel : BaseModel
{
    public CheckoutShippingAddressModel CheckoutShippingAddressModel { get; set; } = new();

}