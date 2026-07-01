using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Data;
using EShop.Core.Platform.Logging.Services;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Extensions;
using EShop.Web.Common.Controllers;
using EShop.Web.Models.Catalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;

public class ProductController : EShopBaseController
{
    private readonly ApplicationDbContext _db;
    private readonly CatalogHelper _catalogHelper;
    private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
    private readonly IActivityLogger _activityLogger;

    public ProductController(ApplicationDbContext db, CatalogHelper catalogHelper,
        IRecentlyViewedProductsService recentlyViewedProductsService, IActivityLogger activityLogger)
    {
        _db = db;
        _catalogHelper = catalogHelper;
        _recentlyViewedProductsService = recentlyViewedProductsService;
        _activityLogger = activityLogger;
    }


   
    public async Task<IActionResult> ProductDetails(int productId, ProductVariantQuery query)
    {
        Product product = await _db
            .Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Brand)
            .Include(x => x.MediaFiles)
            .FirstOrDefaultAsync(x => x.Id == productId);

        if (product == null)
        {
            return NotFound();
        }

        var model = await _catalogHelper.MapProductDetailsPageModelAsync(product, query);
        _recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

        _activityLogger.InsertActivity(KnownActivityLogType.ViewProduct,
            "ActivityLog.PublicActivity.ViewProduct",
            product);

        return View("Product", model);
    }

    // public async Task<IActionResult> UpdateProductDetailsInList(int productId, ProductVariantQuery query)
    // {
    //     (Product product, int quantity) = await ExtractProductFromForm(HttpContext.Request.Form, productId);
    //     var model = new ProductSummaryModel();
    //     // var ctx = _catalogHelper.CreateModelContext()
    //     await _catalogHelper.PrepareProductSummaryModelAsync();
    // }
    
    [HttpPost]
    public async Task<IActionResult> UpdateProductDetails(int productId, ProductVariantQuery productVariantQuery)
    {
        (Product product, int quantity) = await ExtractProductFromForm(HttpContext.Request.Form, productId);
    
        var model = new ProductDetailModel();
        var ctx = _catalogHelper.CreateModelContext(product, productVariantQuery);
        await _catalogHelper.PrepareProductDetailModelAsync(model, ctx, quantity);
 
        object partials = new
        {
            Price = await RenderPartialViewToStringAsync("_Product.Price", model),
            Variants = await RenderPartialViewToStringAsync("_Product.Variants", model.ProductVariantAttributes),
            Labels = await RenderPartialViewToStringAsync("_Product.Labels", model.Labels),
            Stock = await RenderPartialViewToStringAsync("_Product.Stock", model),
            AddToCart = await RenderPartialViewToStringAsync("_Product.AddToCart", model),
        };

        return new JsonResult(new
        {
            Partials = partials
        });
    }
    
    private async Task<(Product product, int quantity)> ExtractProductFromForm(IFormCollection form, int productId)
    {
        var quantity = 1;
        var product = await _db.Products.FindByIdAsync(productId);
        var quantityKey = form.Keys.FirstOrDefault(x => x.EndsWith("quantity"));
        if (!quantityKey.IsEmpty())
        {
            int.TryParse(form[quantityKey], out quantity);
        }

        return (product, quantity);
    }

}