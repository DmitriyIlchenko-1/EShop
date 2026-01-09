using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Data;
using EShop.Core.Platform.Logging.Services;
using EShop.Web.Models.Catalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;


public class ProductController : Controller
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


    public async Task<IActionResult> ProductDetails(int id, ProductVariantQuery query)
    {
        Product product = await _db
            .Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Brand)
            .Include(x => x.ProductMedias)
            .ThenInclude(x => x.MediaFile)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        var model = await _catalogHelper.MapProductDetailsPageModelAsync(product, query);
        _recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

        _activityLogger.InsertActivity(KnownActivityLogType.ViewProduct,
            "ActivityLog.PublicActivity.ViewProduct",
            product);

        return View(model);
    }

    public async Task<IActionResult> UpdateProductDetails(int productId, ProductVariantQuery productVariantQuery)
    {
        var form = HttpContext.Request.Form;
        var quantity = 1;

        var product = await _db
            .Products.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == productId);

        var quantityKey = form.Keys.FirstOrDefault(x => x.EndsWith("EnteredQuantity"));
        if (!string.IsNullOrWhiteSpace(quantityKey))
        {
            _ = int.TryParse(form[quantityKey], out quantity);
        }

        var model = new ProductDetailVm();
        var ctx = _catalogHelper.CreateModelContext(product, productVariantQuery);
        await _catalogHelper.PrepareProductDetailModelAsync(model, ctx, quantity);

        throw new NotImplementedException();
    }
}