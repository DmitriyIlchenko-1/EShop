using EShop.Infrastructure.Domain;
using EShop.Web.Common.Models;

namespace EShop.Web.Models.Checkout;

public class CheckoutPaymentMethodModel : BaseEntity
{
    public bool IsAjaxRequested { get; set; }
    public ICollection<PaymentMethodModel> PaymentMethodModels { get; set; } = [];
}


public class PaymentMethodModel : BaseModel
{
    public string SystemName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Selected { get; set; }
    public string Logo { get; set; }
}