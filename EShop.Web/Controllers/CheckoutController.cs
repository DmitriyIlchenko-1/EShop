using EShop.Core.Common.Domain;
using EShop.Core.Common.Services;
using EShop.Core.Data;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Data.Cart.Services;
using EShop.Core.Data.Settings;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Extensions;
using EShop.Core.Platform.Modules;
using EShop.Infrastructure.Data;
using EShop.Web.Common.Controllers;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace EShop.Web.Controllers;

public class CheckoutController : EShopBaseController
{
    private readonly IShoppingCartService _shoppingCartService;
    private readonly CheckoutSettings _checkoutSettings;
    private readonly IPaymentProviderManager _paymentProviderManager;
    private readonly IWorkContext _workContext;
    private readonly IAddressService _addressService;
    private readonly CheckoutHelper _checkoutHelper;
    private readonly ApplicationDbContext _db;


    public CheckoutController(IShoppingCartService shoppingCartService, IWorkContext workContext,
        CheckoutSettings checkoutSettings, IPaymentProviderManager paymentProviderManager,
        IAddressService addressService, CheckoutHelper checkoutHelper, ApplicationDbContext db)
    {
        _shoppingCartService = shoppingCartService;
        _workContext = workContext;
        _checkoutSettings = checkoutSettings;
        _paymentProviderManager = paymentProviderManager;
        _addressService = addressService;
        _checkoutHelper = checkoutHelper;
        _db = db;
    }


    [Microsoft.AspNetCore.Mvc.HttpGet("checkout", Name = "Checkout")]
    public virtual async Task<IActionResult> Checkout()
    {
        if (!_checkoutSettings.AllowGuestsToOrder && _workContext.CurrentUser.IsGuest())
        {
            return Challenge();
        }

        var user = _workContext.CurrentUser;
        var paymentMethods = _paymentProviderManager.GetActivePaymentMethods();
        if (!paymentMethods.Any())
        {
            return RedirectToRoute("ShoppingCart");
        }

        var cart = await _shoppingCartService.GetUserCartAsync(user);
        if (cart.GetCount() == 0)
        {
           // return RedirectToRoute("ShoppingCart");
        }

        var model = await _checkoutHelper.PrepareCheckoutModelAsync(cart);
        return View(model);
    }

    public virtual async Task<IActionResult> UpdateShippingAddress(CheckoutShippingAddressModel model)
    {
        var user = _workContext.CurrentUser;
        if (!_checkoutSettings.AllowGuestsToOrder && user.IsGuest())
        {
            return Challenge();
        }

        var cart = await _shoppingCartService.GetUserCartAsync();
        if (cart.GetCount() == 0)
        {
        }

        var addressModel = model.NewAddress;
        var address = await _addressService.GetUserAddressById(user.Id, addressModel.Id);
        if (address == null)
        {
        }

        if (!ModelState.IsValid)
        {
            return await GetResultAfterInvalidValidationUpdate(model);
        }

        address.FirstName = addressModel.FirstName;
        address.LastName = addressModel.LastName;
        address.PhoneNumber = addressModel.PhoneNumber;
        address.AddressLine1 = addressModel.AddressLine1;
        address.AddressLine2 = addressModel.AddressLine2;
        address.ZipCode = addressModel.ZipCode;
        address.CityId = addressModel.CityId;


        _db.Addresses.Update(address);
        user.ShippingAddressId = address.Id;
        await _db.SaveChangesAsync();


        return await GetResultAfterSuccessUpdate(cart);
    }

    public virtual async Task<IActionResult> SaveAddressCheckout(CheckoutShippingAddressModel model)
    {
        var user = _workContext.CurrentUser;
        var cart = await _shoppingCartService.GetUserCartAsync(user);

        var result =  await ValidateCheckoutFlow(user, cart);
        if (result.ActionResult is not EmptyResult)
        {
            return result.ActionResult;
        }


        var newAddress = model.NewAddress;
        if (!ModelState.IsValid)
        {
            return await GetResultAfterInvalidValidationUpdate(model);
        }

        var existingAddresses = await _addressService.GetAddressesByUserIdAsync(user.Id, false);
        var address = newAddress.ToEntity();
        var existingAddressWithTheSameFields = existingAddresses.SingleOrDefault(x => x == address);
        if (existingAddressWithTheSameFields == null)
        {
            user.Addresses.Add(address);
            await _db.SaveChangesAsync();
            user.ShippingAddressId = address.Id;
        }

        return await GetResultAfterSuccessUpdate(cart);
    }

    public virtual async Task<IActionResult> GetAddressById(int addressId)
    {
        Address address = null;
        if (addressId != 0)
        {
            address = await _addressService.GetUserAddressById(_workContext.CurrentUser.Id, addressId);
        }

        var model = new AddressModel();
        await _checkoutHelper.PrepareAddressModelAsync(model, address, _workContext.CurrentUser, true);
        var json = JsonConvert.SerializeObject(model,
            Formatting.Indented,
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        return Json(json);
    }

    [HttpDelete]
    public virtual async Task<IActionResult> RemoveAddress(int addressId)
    {
        var user = _workContext.CurrentUser;
        var cart = await _shoppingCartService.GetUserCartAsync(user);
        if (!(cart.GetCount() > 0))
        {
        }

        // temp
        _db
            .Entry(user)
            .Collection(x => x.Addresses)
            .Load();
        var address = user.Addresses.FirstOrDefault(x => x.Id == addressId);
        if (address != null)
        {
            Console.WriteLine($"Address with id {addressId} has been found");
            user.Addresses.Remove(address);
            _db.Addresses.Remove(address);
            await _db.SaveChangesAsync();
            Console.WriteLine($"Address with id {addressId} has been removed");
        }
        Console.WriteLine($"Address with id {addressId} has not been found");

        var model = new CheckoutShippingAddressModel();
        await _checkoutHelper.PrepareShippingAddressModelAsync(model, cart, true);
        return Json(new
        {
            renderSections = new
            {
                address = await RenderPartialViewToStringAsync("_Checkout.Address", model),
                payment = await RenderComponentToStringAsync("CheckoutPaymentMethodList")
            }
        });
    }

    [HttpPost]
    public virtual async Task<IActionResult> SaveOrder()
    {
        var user = _workContext.CurrentUser;
        var cart = await _shoppingCartService.GetUserCartAsync(user);
        if (cart.GetCount() == 0)
        {
            throw new InvalidOperationException("Your cart's empty");
        }

        if (!_checkoutSettings.AllowGuestsToOrder && user.IsGuest())
        {
            return Challenge();
        }

        throw new NotImplementedException();
    }

    protected virtual Task<ValidationFlowResult> ValidateCheckoutFlow(User user, ShoppingCart shoppingCart)
    {
        if (!_checkoutSettings.AllowGuestsToOrder && user.IsGuest())
        {
            return Task.FromResult(new ValidationFlowResult(Challenge()));
        }
        
        if (shoppingCart.GetCount() == 0)
        {
            throw new InvalidOperationException("Cart can not be empty");
        }

        return Task.FromResult(ValidationFlowResult.Empty);
    }

    private async Task<IActionResult> GetResultAfterSuccessUpdate(ShoppingCart cart)
    {
        var model = new CheckoutShippingAddressModel();  
        await _checkoutHelper.PrepareShippingAddressModelAsync(model, cart, true);
        return Json(new
        {
            renderSections = new
            {
                selectAddress = await RenderPartialViewToStringAsync("_Checkout.SelectAddress", model),
                payment = await RenderComponentToStringAsync("CheckoutPaymentMethodList")
            },
            success = true
            
        });
    }

    private async Task<IActionResult> GetResultAfterInvalidValidationUpdate(CheckoutShippingAddressModel model)
    {
        // Repopulate ViewData so that HtmlFieldPrefix is updated before we render _Checkout.CreateOrUpdateAddress.cshtml partial view
        // because otherwise there will be an out-of-sync thing with the field (form control) names generated by the input tag helpers and the names found in ModelState reporting validation errors.
        var dataDictNewAddress = new ViewDataDictionary(ViewData);
        dataDictNewAddress.TemplateInfo.HtmlFieldPrefix = nameof(model.NewAddress);
        await _checkoutHelper.PrepareAddressModelAsync(model.NewAddress, null, null, true, true);
        return Json(new
        {
            renderSections = new
            {
                addUpdateAddressForm = await RenderPartialViewToStringAsync("_Checkout.CreateOrUpdateAddress",
                    model.NewAddress,
                    dataDictNewAddress)
            },
            success = false,
        });
        
        
    }


    public class ValidationFlowResult
    {
        public static ValidationFlowResult Empty = new(ControllerBase.Empty);
        
        public ValidationFlowResult(ActionResult actionResult)
        {
            ActionResult = actionResult;
        }
        public IActionResult ActionResult { get; }
    }
}
