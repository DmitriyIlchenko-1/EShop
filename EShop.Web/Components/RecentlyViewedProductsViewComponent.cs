using EShop.Core.Catalog.Configuration;
using EShop.Core.Catalog.Products.Services;
using EShop.Infrastructure.Extensions;
using EShop.Web.Common.Conponents;
using EShop.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EShop.Web.Components;

public class RecentlyViewedProductsViewComponent : BaseViewComponent
{
    private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
    private readonly CatalogHelper _catalogHelper;
    private readonly CatalogSettings _catalogSettings;


    public RecentlyViewedProductsViewComponent(IRecentlyViewedProductsService recentlyViewedProductsService,
        CatalogHelper catalogHelper, CatalogSettings catalogSettings)
    {
        _recentlyViewedProductsService = recentlyViewedProductsService;
        _catalogHelper = catalogHelper;
        _catalogSettings = catalogSettings;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var routeData = HttpContext.GetRouteData();
        var routeString = routeData.Values.GetRouteString();
        int? currentProductId = routeString == "Product.ProductDetails"
            ? routeData.Values.GetValueOrDefaultAs<int>("productId")
            : null;

        var products =
            await _recentlyViewedProductsService.GetRecentlyViewedProducts(
                _catalogSettings.RecentlyViewedProductsNumber,
                currentProductId);

        if (!products.Any())
        {
            return NoContent();
        }

        return View(await _catalogHelper.PrepareProductSummaryModelAsync(products));
    }
}