using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Data;
using EShop.Core.Data.Cart.Services;
using EShop.Core.Platform.Logging.Services;
using EShop.Infrastructure.Caching;
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
    readonly CheckoutHelper _checkoutHelper;
    readonly IRequestCache _requestCache;

    public ShoppingCartController(ApplicationDbContext db, IShoppingCartService shoppingCartService,
        IActivityLogger activityLogger, IRequestCache requestCache, CheckoutHelper checkoutHelper)
    {
        _db = db;
        _shoppingCartService = shoppingCartService;
        _activityLogger = activityLogger;
        _requestCache = requestCache;
        _checkoutHelper = checkoutHelper;
    }

    [HttpGet("/cart", Name = "ShoppingCart")]
    public async Task<IActionResult> Cart()
    {
        var cart = await _shoppingCartService.GetUserCartAsync();
        var model = await _checkoutHelper.PrepareShoppingCartModelAsync(cart);
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

        (bool result, ICollection<string> errors) = await _shoppingCartService.AddProductToCart(addToCartContext);

        if (!result)
        {
            return Json(new
            {
                productId,
                success = false,
                errors,
            });
        }

        _activityLogger.InsertActivity(KnownActivityLogType.AddToShoppingCart,
            KnownActivityFormats.AddToShoppingCart,
            product.Name);

        var cartCount = await _shoppingCartService.GetUserCartItemCountAsync();
        return Json(new
        {
            productId,
            success = true,
            partials = new
            {
                AddToCartCount = await RenderComponentToStringAsync("AddToCartCount", cartCount)
            }
        });
    }


    [HttpPost]
    public async Task<IActionResult> UpdateCartItem(UpdateCartItemModel model)
        => await UpdateCartItemInternal(model);

    private async Task<IActionResult> UpdateCartItemInternal(UpdateCartItemModel model, bool delete = false)
    {
        string message = string.Empty;
        bool success = false;
        string cartHtml = string.Empty;
        string totalSummaryHtml = string.Empty;
        string cartCountHtml = string.Empty;
        
        var cartItem = await _db.ShoppingCartItems.FirstOrDefaultAsync(x => x.Id == model.CartItemId);
        if (cartItem is null)
        {
            return Json(new
            {
                success = false,
                error = "Failed to perform the action."
            });
        }
        
        var (result, warnings) =
            await _shoppingCartService.UpdateCartItemAsync(cartItem, delete ? 0 :  model.NewQuantity);
        message = string.Join(". ", warnings.Take(2));
        success = result;

        if (!success)
        {
            return Json(new
            {
                success,
                message
            });
        }
        
        var cart = await _shoppingCartService.GetUserCartAsync();
        var cartModel = await _checkoutHelper.PrepareShoppingCartModelAsync(cart);
        cartHtml = await RenderPartialViewToStringAsync("_ShoppingCart.CartItems", cartModel);
        totalSummaryHtml = await RenderPartialViewToStringAsync("_ShoppingCart.TotalSummary", cartModel);
        cartCountHtml = await RenderComponentToStringAsync("AddToCartCount", cart.GetCount());
        return Json(new
        {
            success,
            cartHtml,
            totalSummaryHtml,
            cartCountHtml

        });
    }
}