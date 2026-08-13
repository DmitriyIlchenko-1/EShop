using EShop.Core.Data.Cart.Services;
using EShop.Web.Common.Conponents;
using EShop.Web.Controllers;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Web.Components;

public class CartDrawerViewComponent : BaseViewComponent
{
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ShoppingCartHelper _shoppingCartHelper;

    public CartDrawerViewComponent(IShoppingCartService shoppingCartService, ShoppingCartHelper shoppingCartHelper)
    {
        _shoppingCartService = shoppingCartService;
        _shoppingCartHelper = shoppingCartHelper;
    }

    public async Task<IViewComponentResult> InvokeAsync(ShoppingCartModel? model)
    {
        if (model is null)
        {
            var cart = await _shoppingCartService.GetUserCartAsync(); 
            model = await _shoppingCartHelper.PrepareShoppingCartModelAsync(cart);
        }
        return View(model);
    }
}