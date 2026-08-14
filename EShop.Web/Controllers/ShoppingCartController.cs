using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Data;
using EShop.Core.Data.Cart.Services;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Logging.Services;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data;
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

    [HttpPost("/cart/addproductsimple/{productId:int}")]
    public async Task<IActionResult> AddProductSimple(int productId)
    {
        var product = await _db
            .Products
            .Include(x => x.ProductVariantAttributes)
            .FindByIdAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        var addToCartContext = new AddToCartContext()
        {
            Product = product,
            Quantity = product.MinAddToCartNumber,
            VariantQuery = new ProductVariantQuery()
        };

        var (warnings, isAdded) = await _shoppingCartService.AddProductToCart(addToCartContext);
        if (!isAdded)
        {
            // The user has to visit the product's page to more likely fix whatever to add the product to their cart
            return RedirectToAction(nameof(ProductController.ProductDetails),
                "Product",
                new { productId });
        }

        _activityLogger.InsertActivity(KnownActivityLogType.AddToShoppingCart,
            KnownActivityFormats.AddToShoppingCart,
            product.Name);

        var cart = await _shoppingCartService.GetUserCartAsync();
        var shoppingCartModel = await _shoppingCartHelper.PrepareShoppingCartModelAsync(cart);
        var cartCountModel = await _shoppingCartService.GetUserCartItemCountAsync();

        return Json(new
        {
            productId,
            warnings,
            partials = new
            {
                cartDrawer = await RenderComponentToStringAsync("CartDrawer", shoppingCartModel),
                addToCartCount = await RenderComponentToStringAsync("AddToCartCount", cartCountModel),
            }
        });
    }

    [HttpPost("/cart/addproduct/{productId:int}", Name = "AddProduct")]
    public async Task<IActionResult> AddProduct(int productId, ProductVariantQuery query)
    {
       
        var product = await _db
            .Products
            .Include(x => x.ProductVariantAttributes)
            .FirstOrDefaultAsync(x => x.Id == productId);
        if (product is null)
        {
            return RedirectToRoute("Homepage");
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

        var (warnings, _) = await _shoppingCartService.AddProductToCart(addToCartContext);


        _activityLogger.InsertActivity(KnownActivityLogType.AddToShoppingCart,
            KnownActivityFormats.AddToShoppingCart,
            product.Name);

        var cartCount = await _shoppingCartService.GetUserCartItemCountAsync();
        var cart = await _shoppingCartService.GetUserCartAsync();
        var settings = new ShoppingCartModelMappingSettings();
        _shoppingCartHelper.GetBestFitShoppingCartModelMappingSettings(settings, CartSummaryLocation.CartDrawer);
        var cartModel = await _shoppingCartHelper.PrepareShoppingCartModelAsync(cart, settings);
        return Json(new
        {
            productId,
            warnings,
            partials = new
            {
                addToCartCount = await RenderComponentToStringAsync("AddToCartCount", cartCount),
                cartDrawer = await RenderComponentToStringAsync("CartDrawer", cartModel),
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
        string[] requestedPartials = model.RequestedPartials;
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

        IDictionary<string, string> partials = new Dictionary<string, string>();

        var cartCount = cart.GetCount();
        partials.Add("addToCartCount", await RenderComponentToStringAsync("AddToCartCount", cartCount));

        if (requestedPartials.Contains("cartDrawer", StringComparer.InvariantCultureIgnoreCase))
        {
            var shoppingCartModel = await _shoppingCartHelper.PrepareShoppingCartModelAsync(cart);
            partials.Add("cartDrawer", await RenderComponentToStringAsync("CartDrawer", shoppingCartModel));
        }

        if (requestedPartials.Contains("totalSummary", StringComparer.InvariantCultureIgnoreCase))
        {
            var cartModel = await _shoppingCartHelper.PrepareShoppingCartModelAsync(cart);
            partials.Add("totalSummary", await RenderPartialViewToStringAsync("_ShoppingCart.TotalSummary", cartModel));
        }

        if (requestedPartials.Contains("cart", StringComparer.InvariantCultureIgnoreCase))
        {
            var cartModel = await _shoppingCartHelper.PrepareShoppingCartModelAsync(cart);
            partials.Add("cart", await RenderPartialViewToStringAsync("Cart", cartModel));
        }
        

        return Json(new
        {
            success = true,
            message,
            partials,
            cartCount 
        });
    }
}