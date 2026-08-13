using EShop.Core.Common.Domain;
using EShop.Core.Common.Services;
using EShop.Core.Platform.Common;
using EShop.Infrastructure.Utilities;
using EShop.Web.Mappers;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EShop.Web.Factories;

public class DefaultAddressModelFactory : IAddressModelFactory
{
    private readonly IWorkContext _workContext;
    private readonly ICityService _cityService;

    public DefaultAddressModelFactory(IWorkContext workContext, ICityService cityService)
    {
        _workContext = workContext;
        _cityService = cityService;
    }

    public virtual async Task PrepareAddressModelAsync(AddressModel model, Address address,
        bool prepopulateWithUserData = false, bool loadCities = false)
    {
        Guard.NotNull(model);
        var user = _workContext.CurrentUser;
        if (address != null)
        {
            address.ToAddressModel(model);
            model.IsDefault = user.ShippingAddressId.HasValue && user.ShippingAddressId == address.Id;
        }

        if (prepopulateWithUserData)
        {
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.PhoneNumber = user.PhoneNumber;
        }

        if (loadCities)
        {
            var cities = await _cityService.GetAllAsync();
            model.AvailableCities.Add(new SelectListItem()
            {
                Text = "Select city",
                Value = "0",
            });

            foreach (var city in cities)
            {
                model.AvailableCities.Add(new SelectListItem()
                {
                    Text = city.Name,
                    Value = city.Id.ToString(),
                    Selected = model.CityId == city.Id
                });
            }
        }
    }
}

