using EShop.Core.Common.Domain;
using EShop.Web.Models.Checkout;

namespace EShop.Web.Factories;

public interface IAddressModelFactory
{
    Task PrepareAddressModelAsync(AddressModel model, Address address,
        bool prepopulateWithUserData = false, bool loadCities = false);
}