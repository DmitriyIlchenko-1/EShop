using EShop.Core.Common.Domain;
using EShop.Core.Common.Services;
using EShop.Core.Data;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Modules;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Utilities;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;

public partial class CheckoutHelper
{
    private readonly IWorkContext _workContext;
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestCache _requestCache;
    private readonly IAddressService _addressService;
    private readonly ICityService _cityService;
    private readonly IPaymentProviderManager _paymentProviderManager;
    private const string UserAddressesCacheKey = "user:addresses:{0}";

    public CheckoutHelper(IWorkContext workContext, IAddressService addressService, IRequestCache requestCache,
        ApplicationDbContext dbContext, ICityService cityService, IPaymentProviderManager paymentProviderManager)
    {
        _workContext = workContext;
        _addressService = addressService;
        _requestCache = requestCache;
        _dbContext = dbContext;
        _cityService = cityService;
        _paymentProviderManager = paymentProviderManager;
    }

    public virtual async Task<CheckoutModel> PrepareCheckoutModelAsync(ShoppingCart cart)
    {
        Guard.NotNull(cart);
        var model = new CheckoutModel();
        //TODO: when we first go to checkout and we do have an existing address, we don't (do we???) want to populate NewAddress with the user's fields cuz there's no need for it. We can do this when we click the add btn to display the form to add a new address. Another thing is do we need to refetch the data (populateWithUserData:true) for the new address every time we click the add btn? 
        await PrepareShippingAddressModelAsync(model.CheckoutShippingAddressModel, cart, false);
        return model;
    }

    public virtual async Task PrepareShippingAddressModelAsync(CheckoutShippingAddressModel model, ShoppingCart cart,
        bool prepopulateWithUserData = false)
    {
        Guard.NotNull(model);
        Guard.NotNull(cart);
        var user = _workContext.CurrentUser;
        model.ExistingAddresses ??= new List<AddressModel>();
        foreach (var address in user.Addresses.OrderBy(x => x.Id))
        {
            var addressModel = new AddressModel();
            await PrepareAddressModelAsync(addressModel, address, user);
            model.ExistingAddresses.Add(addressModel);
        }

        model.NewAddress ??= new AddressModel();
        await PrepareAddressModelAsync(model.NewAddress, null, user, true);
    }


    public virtual async Task PrepareAddressModelAsync(AddressModel model, Address address, User user,
        bool prepopulateWithUserData = false, bool disableAddressBaseMapping = false)
    {
        Guard.NotNull(model);
        if (address != null && !disableAddressBaseMapping)
        {
            model.Id = address.Id;
            model.Selected = user.ShippingAddressId == address.Id;
            model.AddressString = address.ToString();
            model.FirstName = address.FirstName;
            model.LastName = address.LastName;
            model.PhoneNumber = address.PhoneNumber;
            model.AddressLine1 = address.AddressLine1;
            model.AddressLine2 = address.AddressLine2;
            model.ZipCode = address.ZipCode;
            model.CityId = address.CityId.HasValue ? address.CityId.Value : 0;
        }
        else if (prepopulateWithUserData)
        {
            if (!disableAddressBaseMapping)
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user),
                        "User cannot be null to populate address with user data");
                }

                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.PhoneNumber = user.PhoneNumber;

                if (!(user.Addresses.Count > 0))
                {
                    model.NeedsCreating = true;
                }
               
            }

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

    public virtual CheckoutPaymentMethodModel PreparePaymentModelAsync()
    {
        var model = new CheckoutPaymentMethodModel();
        var user = _workContext.CurrentUser;
        foreach (var provider in _paymentProviderManager.GetActivePaymentMethods())
        {
            var metadata = provider.Metadata;
            var prModel = new PaymentMethodModel()
            {
                Name = metadata.FriendlyName,
                SystemName = metadata.SystemName
            };
            model.PaymentMethodModels.Add(prModel);
        }

        if (model.PaymentMethodModels.FirstOrDefault(x => x.Selected) == null)
        {
            var first = model.PaymentMethodModels.FirstOrDefault(x => x.Selected);
            if (first != null)
            {
                first.Selected = true;
            }
        }

        return model;
    }
}