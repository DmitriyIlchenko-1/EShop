using EShop.Core.Common.Domain;
using EShop.Core.Common.Services;
using EShop.Core.Data;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Data.Cart.Services;
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
    private readonly ISession _session;
    private readonly IAddressService _addressService;
    private readonly ICityService _cityService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IPaymentProviderManager _paymentProviderManager;
    private const string UserAddressesCacheKey = "user:addresses:{0}";

    public CheckoutHelper(IWorkContext workContext, IAddressService addressService, IHttpContextAccessor contextAccessor,
        ApplicationDbContext dbContext, ICityService cityService, IPaymentProviderManager paymentProviderManager, IShoppingCartService shoppingCartService)
    {
        _workContext = workContext;
        _addressService = addressService;
        _session = contextAccessor.HttpContext?.Session;
        _dbContext = dbContext;
        _cityService = cityService;
        _paymentProviderManager = paymentProviderManager;
        _shoppingCartService = shoppingCartService;
    }

    public virtual async Task<CheckoutModel> PrepareCheckoutModelAsync(ShoppingCart cart)
    {
        Guard.NotNull(cart);
        var model = new CheckoutModel();
        //TODO: when we first go to checkout and we do have an existing address, we don't (do we???) want to populate NewAddress with the user's fields cuz there's no need for it. We can do this when we click the add btn to display the form to add a new address. Another thing is do we need to refetch the data (populateWithUserData:true) for the new address every time we click the add btn? 
        model.CheckoutShippingAddressModel = await PrepareShippingAddressModelAsync(cart, false);
        return model;
    }

    public virtual async Task<CheckoutShippingAddressModel> PrepareShippingAddressModelAsync(ShoppingCart cart,
        bool prepopulateWithUserData = false,  bool disableAddressBaseMapping = false)
    {
        Guard.NotNull(cart);
        var user = _workContext.CurrentUser;
        var model = new CheckoutShippingAddressModel();
        foreach (var address in user.Addresses.OrderBy(x => x.Id))
        {
            var addressModel = new AddressModel();
            await PrepareAddressModelAsync(addressModel, address);
            model.ExistingAddresses.Add(addressModel);
        }
        
        await PrepareAddressModelAsync(model.NewAddress, null, prepopulateWithUserData, disableAddressBaseMapping);
        return model;
    }


    public virtual async Task PrepareAddressModelAsync(AddressModel model, Address address, 
        bool prepopulateWithUserData = false, bool disableAddressBaseMapping = false)
    {
        Guard.NotNull(model);
        var user = _workContext.CurrentUser;
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
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.PhoneNumber = user.PhoneNumber;
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
            var first = model.PaymentMethodModels.FirstOrDefault();
            if (first != null)
            {
                first.Selected = true;
            }
        }

        return model;
    }

    public virtual async Task<CheckoutDataSummaryModel> PrepareCheckoutDataSummaryModelAsync()
    {
        var user = _workContext.CurrentUser;
        var model = new CheckoutDataSummaryModel();
        var shippingAddress = user.ShippingAddress;
        await PrepareAddressModelAsync(model.ShippingAddress, shippingAddress);

        if (_session != null)
        {
            var paymentMethodName = _session.GetString("PaymentMethod");
            var paymentMethod = _paymentProviderManager.GetActivePaymentMethod(paymentMethodName)
                ?.Metadata.FriendlyName ?? string.Empty;
            model.PaymentMethodName = paymentMethod;
        }

        return model;
    }
}