using EShop.Web.Common.Models;
using EShop.Web.Models.Account;

namespace EShop.Web.Models.Checkout;

public class CheckoutDataSummaryModel : BaseModel
{
    public AddressModel ShippingAddress { get; set; } = new();
    public string PaymentMethodName { get; set; }
    
}