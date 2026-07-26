using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Data;
using EShop.Core.Data.Cart.Services;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Logging.Services;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Extensions;
using EShop.Web.Common.Controllers;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;

public class ShoppingCartController : EShopBaseController
{
    private readonly ApplicationDbContext _db;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IActivityLogger _activityLogger;
    readonly ShoppingCartHelper _shoppingCartHelper;
    readonly IRequestCache _requestCache;
    readonly IWorkContext _workContext;

    public ShoppingCartController(ApplicationDbContext db, IShoppingCartService shoppingCartService,
        IActivityLogger activityLogger, IRequestCache requestCache, ShoppingCartHelper shoppingCartHelper,
        IWorkContext workContext)
    {
        _db = db;
        _shoppingCartService = shoppingCartService;
        _activityLogger = activityLogger;
        _requestCache = requestCache;
        _shoppingCartHelper = shoppingCartHelper;
        _workContext = workContext;
    }

    [HttpGet("/cart", Name = "ShoppingCart")]
    public async Task<IActionResult> Cart()
    {
        var cart = await _shoppingCartService.GetUserCartAsync();
        var model = await _shoppingCartHelper.PrepareShoppingCartModelAsync(cart);
        return View(model);
    }

    [HttpPost("/card/addproduct/{productId:int}", Name = "AddProduct")]
    public async Task<IActionResult> AddProduct(int productId, ProductVariantQuery query)
    {
        var product = await _db
            .Products
            .Include(x => x.ProductVariantAttributes)
            .FirstOrDefaultAsync(x => x.Id == productId);
        if (product is null)
        {
            return Json(new
            {
                redirectUrl = Url.RouteUrl("Homepage")
            });
        }

        var form = HttpContext.Request.Form;
        int quantity = 0;
        if (form.ContainsKey("quantity"))
        {
            quantity = int.Parse(form["quantity"]);
        }

        var addToCartContext = new AddToCartContext()
        {
            Product = product,
            Quantity = quantity,
            VariantQuery = query
        };

        ICollection<string> warnings = await _shoppingCartService.AddProductToCart(addToCartContext);


        _activityLogger.InsertActivity(KnownActivityLogType.AddToShoppingCart,
            KnownActivityFormats.AddToShoppingCart,
            product.Name);

        var cartCount = await _shoppingCartService.GetUserCartItemCountAsync();
        return Json(new
        {
            productId,
            warnings,
            partials = new
            {
                addToCartCount = await RenderComponentToStringAsync("AddToCartCount", cartCount)
            }
        });
    }


    [HttpPost]
    public async Task<IActionResult> UpdateCartItem(UpdateCartItemModel model)
        => await UpdateCartItemInternal(model);

    [HttpPost]
    public async Task<IActionResult> RemoveCartItem(UpdateCartItemModel model)
        => await UpdateCartItemInternal(model, true);

    private async Task<IActionResult> UpdateCartItemInternal(UpdateCartItemModel model, bool delete = false)
    {
        string message = string.Empty;
        var cart = await _shoppingCartService.GetUserCartAsync();
        var cartItem = cart.Items.FirstOrDefault(x => x.Id == model.CartItemId);
        if (cartItem is null)
        {
            return Json(new
            {
                success = false,
                message = "Failed to perform the action."
            });
        }

        if (delete || model.NewQuantity == 0)
        {
            await _shoppingCartService.RemoveCartItemAsync(cartItem);
        }
        else
        {
            var warnings = await _shoppingCartService.UpdateCartItemAsync(cartItem, model.NewQuantity);
            message = string.Join(". ", warnings.Take(2));
        }
        
        var cartCount = cart.GetCount();
        var cartModel = await _shoppingCartHelper.PrepareShoppingCartModelAsync(cart);
        return Json(new
        {
            success = true,
            message,
            cartHtml = await RenderPartialViewToStringAsync("_ShoppingCart.CartItems", cartModel),
            totalSummaryHtml = await RenderPartialViewToStringAsync("_ShoppingCart.TotalSummary", cartModel),
            cartCountHtml = await RenderComponentToStringAsync("AddToCartCount", cartCount),
            cartCount
        });
    }
}