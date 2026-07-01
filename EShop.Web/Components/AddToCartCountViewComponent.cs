using EShop.Core.Data.Cart.Services;
using EShop.Infrastructure.Caching;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Web.Components;

public class AddToCartCountViewComponent : ViewComponent
{
    readonly IShoppingCartService _shoppingCartService;
    private readonly IRequestCache _requestCache;
    public AddToCartCountViewComponent(IShoppingCartService shoppingCartService, IRequestCache requestCache)
    {
        _shoppingCartService = shoppingCartService;
        _requestCache = requestCache;
    }

    public async Task<IViewComponentResult> InvokeAsync(int? count)
    {
        count ??= await _shoppingCartService.GetUserCartItemCountAsync();
        return View(count.Value);
    }
}