using EShop.Core.Checkout.Orders.Services;
using EShop.Core.Common.Domain;
using EShop.Core.Common.Services;
using EShop.Core.Data;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Data.Cart.Services;
using EShop.Core.Data.Orders.Extensions;
using EShop.Core.Data.Payment;
using EShop.Core.Data.Settings;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Extensions;
using EShop.Core.Platform.Modules;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Extensions;
using EShop.Web.Common.Controllers;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EShop.Web.Controllers;

public class CheckoutController : EShopBaseController
{
    private readonly IShoppingCartService _shoppingCartService;
    private readonly CheckoutSettings _checkoutSettings;
    private readonly IPaymentProviderManager _paymentProviderManager;
    private readonly IWorkContext _workContext;
    private readonly IOrderService _orderService;
    private readonly CheckoutHelper _checkoutHelper;
    private readonly ApplicationDbContext _db;
    private readonly PaymentSettings _paymentSettings;

    public CheckoutController(IShoppingCartService shoppingCartService, IWorkContext workContext,
        CheckoutSettings checkoutSettings, IPaymentProviderManager paymentProviderManager,
        IAddressService addressService, CheckoutHelper checkoutHelper, ApplicationDbContext db,
        IOrderService orderService, PaymentSettings paymentSettings)
    {
        _shoppingCartService = shoppingCartService;
        _workContext = workContext;
        _checkoutSettings = checkoutSettings;
        _paymentProviderManager = paymentProviderManager;
        _checkoutHelper = checkoutHelper;
        _db = db;
        _orderService = orderService;
        _paymentSettings = paymentSettings;
    }


    [HttpGet("checkout", Name = "Checkout")]
    public virtual async Task<IActionResult> Checkout()
    {
        var result = await ValidateCheckoutFlowAsync();
        if (result.ActionResult != Empty)
        {
            return result.ActionResult;
        }

        var paymentMethods = _paymentProviderManager.GetActivePaymentMethods();
        if (!paymentMethods.Any())
        {
            return RedirectToRoute("ShoppingCart");
        }

        return RedirectToAction(nameof(ShippingAddress));
    }

    public virtual async Task<IActionResult> ShippingAddress()
    {
        var result = await ValidateCheckoutFlowAsync();
        if (result.ActionResult != Empty)
        {
            return result.ActionResult;
        }

        var user = _workContext.CurrentUser;
        var cart = await _shoppingCartService.GetUserCartAsync(user);
        var model = await _checkoutHelper.PrepareShippingAddressModelAsync(cart);
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SelectShippingAddress(int shippingAddressId)
    {
        var result = await ValidateCheckoutFlowAsync();
        if (result.ActionResult != Empty)
        {
            return result.ActionResult;
        }

        var user = _workContext.CurrentUser;
        var address = user.Addresses.FirstOrDefault(x => x.Id == shippingAddressId);
        if (address == null)
        {
        }

        user.ShippingAddress = address;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(PaymentList));
    }

    [HttpPost, ActionName("ShippingAddress")]
    public virtual async Task<IActionResult> AddShippingAddress(CheckoutShippingAddressModel model)
    {
        var validationResult = await ValidateCheckoutFlowAsync();
        if (validationResult.ActionResult != Empty)
        {
            return validationResult.ActionResult;
        }

        var user = _workContext.CurrentUser;
        
        if (ModelState.IsValid)
        {
            var newAddress = model.NewAddress.ToEntity();
            var existingAddress = user.Addresses.FirstOrDefault(x => x == newAddress);
            if (existingAddress != null)
            {
            }
            
            user.ShippingAddress = newAddress;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(PaymentList));
        }

        return View(model);
    }

    public virtual async Task<IActionResult> PaymentList()
    {
        var result = await ValidateCheckoutFlowAsync();
        if (result.ActionResult != Empty)
        {
            return result.ActionResult;
        }

        var model = _checkoutHelper.PreparePaymentModelAsync();
        return View("PaymentList", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SelectPaymentMethod(string paymentSystemName)
    {
        var result = await ValidateCheckoutFlowAsync();
        if (result.ActionResult != Empty)
        {
            return result.ActionResult;
        }

        if (paymentSystemName.IsEmpty())
        {
            return await PaymentList();
        }

        if (!_paymentProviderManager
                .GetActivePaymentMethod(paymentSystemName)
                .IsPaymentMethodActive(_paymentSettings))
        {
            return await PaymentList();
        }

        var session = HttpContext.Session;
        session.SetString("PaymentMethod", paymentSystemName);
        return RedirectToAction(nameof(PaymentInfo));
    }

    public virtual async Task<IActionResult> PaymentInfo()
    {
        var result = await ValidateCheckoutFlowAsync();
        if (result.ActionResult != Empty)
        {
            return result.ActionResult;
        }

        var session = HttpContext.Session;
        var paymentMethodName = session.GetString("PaymentMethod");
        var paymentMethod = _paymentProviderManager.GetActivePaymentMethod(paymentMethodName);
        if (paymentMethod == null)
        {
        }

        if (paymentMethod.Proviver.SkipPaymentInfo)
        {
            return RedirectToAction(nameof(Confirm));
        }

        throw new NotImplementedException();
    }

    public virtual async Task<IActionResult> Confirm()
    {
        var result = await ValidateCheckoutFlowAsync();
        if (result.ActionResult != Empty)
        {
            return result.ActionResult;
        }

        var model = new CheckoutConfirmModel();
        model.SummaryModel = await _checkoutHelper.PrepareCheckoutDataSummaryModelAsync();
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SaveOrder()
    {
        var validationFlowResult = await ValidateCheckoutFlowAsync();
        if (validationFlowResult.ActionResult != Empty)
        {
            return validationFlowResult.ActionResult;
        }

        var paymentRequest = new ProcessPaymentRequest()
        {
            OrderGuid = Guid.NewGuid(),
            PaymentMethodSystemName = HttpContext.Session.GetString("PaymentMethod")
        };
        var result = await _orderService.PlaceOrderAsync(paymentRequest);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Completed), new { orderId = result.Order.Id });
        }

        throw new NotImplementedException();
    }


    public virtual async Task<IActionResult> GetAddressById(int addressId)
    {
        Address address = null;
        var user = _workContext.CurrentUser;
        if (addressId != 0)
        {
            address = user.Addresses.FirstOrDefault(x => x.Id == addressId);
        }

        var model = new AddressModel();
        await _checkoutHelper.PrepareAddressModelAsync(model, address, true);
        var json = JsonConvert.SerializeObject(model,
            Formatting.Indented,
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        return Json(json);
    }

    public virtual async Task<IActionResult> Completed(int orderId)
    {
        var order = await _db
            .Orders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId && x.UserId == _workContext.CurrentUser.Id);
        if (order == null)
        {
            return RedirectToRoute("Homepage");
        }

        return View((object)orderId.ToString());
    }


    private async Task<IActionResult> GetResultAfterSuccessUpdate(ShoppingCart cart)
    {
        var model = await _checkoutHelper.PrepareCheckoutModelAsync(cart);
        return Json(new
        {
            renderSections = new
            {
                address = await RenderPartialViewToStringAsync("_Checkout.Address", model),
                payment = await RenderComponentToStringAsync("CheckoutPaymentMethodList")
            },
            success = true
        });
    }

    protected virtual async Task<ValidationFlowResult> ValidateCheckoutFlowAsync()
    {
        var user = _workContext.CurrentUser;
        var cart = await _shoppingCartService.GetUserCartAsync(user);
        if (!_checkoutSettings.AllowGuestsToOrder && user.IsGuest())
        {
            return new ValidationFlowResult(Challenge());
        }

        if (cart.GetCount() == 0)
        {
            return new ValidationFlowResult(RedirectToRoute("ShoppingCart"));
        }

        return ValidationFlowResult.Empty;
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