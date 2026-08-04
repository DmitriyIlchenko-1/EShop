using EShop.Web.Models.Checkout;

namespace EShop.Web.Models.Account;

public class AddressListModel
{
    public ICollection<AddressModel> Addresses { get; set; } = [];

}

 