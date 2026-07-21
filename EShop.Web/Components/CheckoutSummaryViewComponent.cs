using EShop.Core.Data.Cart.Services;
using EShop.Core.Platform.Common;
using EShop.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Web.Components;

public class CheckoutSummaryViewComponent : ViewComponent
{
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ShoppingCartHelper _shoppingCartHelper;

    public CheckoutSummaryViewComponent(IShoppingCartService shoppingCartService, ShoppingCartHelper shoppingCartHelper)
    {
        _shoppingCartService = shoppingCartService;
        _shoppingCartHelper = shoppingCartHelper;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var shoppingCart = await _shoppingCartService.GetUserCartAsync();
        var mappingSettings = new ShoppingCartModelMappingSettings();
        _shoppingCartHelper.GetBestFitShoppingCartModelMappingSettings(mappingSettings, CartSummaryLocation.CheckoutSummary);
        var model = await _shoppingCartHelper.PrepareShoppingCartModelAsync(shoppingCart, mappingSettings);
        return View(model);
    }
}