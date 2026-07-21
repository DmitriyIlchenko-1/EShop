using EShop.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Web.Components;

public class CheckoutPaymentMethodListViewComponent : ViewComponent
{
    private readonly CheckoutHelper _checkoutHelper;

    public CheckoutPaymentMethodListViewComponent(CheckoutHelper checkoutHelper)
    {
        _checkoutHelper = checkoutHelper;
    }

    public IViewComponentResult Invoke()
    {
        var model = _checkoutHelper.PreparePaymentModelAsync();
        return View(model);
    }
}