using EShop.Web.Common.Models;
using FluentValidation;

namespace EShop.Web.Models.Checkout;

public class CheckoutShippingAddressModel  : BaseModel
{
    public ICollection<AddressModel> ExistingAddresses { get; set; } = [];
    public AddressModel NewAddress { get; set; } = new();
    public bool HasAddresses => ExistingAddresses.Any();
}


public class CheckoutShippingAddressModelValidator : AbstractValidator<CheckoutShippingAddressModel>
{
    public CheckoutShippingAddressModelValidator()
    {
        RuleFor(x => x.NewAddress).SetValidator(new AddressModelValidator());
    }
}