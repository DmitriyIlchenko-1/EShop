
using EShop.Web.Common.Models;
using FluentValidation;

namespace EShop.Web.Models.Checkout;

public class CheckoutModel : BaseModel
{
    public string PaymentMethodSystemName { get; set; }
    public int AddressId { get; set; }
    public CheckoutShippingAddressModel CheckoutShippingAddressModel { get; set; } = new();

} 

public class CheckoutModelValidator : AbstractValidator<CheckoutModel>
{
    public CheckoutModelValidator()
    {
        RuleFor(x => x.PaymentMethodSystemName).NotEmpty();
    }
}