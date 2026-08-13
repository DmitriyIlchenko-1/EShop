using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Checkout.Orders.Domain;
using EShop.Core.Checkout.Orders.Services;
using EShop.Core.Content.Media.Domain;
using EShop.Core.Content.Media.Services;
using EShop.Core.Data;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Data.Settings;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Routing;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;

public class ShoppingCartHelper
{
    private readonly IUrlService _urlService;
    private readonly IUrlHelper _urlHelper;
    readonly CatalogHelper _catalogHelper;
    readonly ApplicationDbContext _db;
    readonly IBrandService _brandService;
    readonly IOrderService _orderService;
    readonly IProductService _productService;
    readonly IMediaService _mediaService;
    private readonly CheckoutSettings _checkoutSettings;
    private readonly IProductAttributeMaterializer _attributeMaterializer;

    public ShoppingCartHelper(IUrlService urlService, IUrlHelper urlHelper, CatalogHelper catalogHelper,
        ApplicationDbContext db, IBrandService brandService, IOrderService orderService, IProductService productService,
        IMediaService mediaService, IProductAttributeMaterializer attributeMaterializer,
        CheckoutSettings checkoutSettings)
    {
        _urlService = urlService;
        _urlHelper = urlHelper;
        _catalogHelper = catalogHelper;
        _db = db;
        _brandService = brandService;
        _orderService = orderService;
        _productService = productService;
        _mediaService = mediaService;
        _attributeMaterializer = attributeMaterializer;
        _checkoutSettings = checkoutSettings;
    }

    public async Task<ShoppingCartModel> PrepareShoppingCartModelAsync(ShoppingCart cart,
        ShoppingCartModelMappingSettings? settings = null)
    {
        Guard.NotNull(cart);

        if (settings == null)
        {
            settings = new ShoppingCartModelMappingSettings();
            //Set ShoppingCart location by default
            GetBestFitShoppingCartModelMappingSettings(settings, CartSummaryLocation.ShoppingCart);
        }

        var model = new ShoppingCartModel();
        var products = cart
            .Items.Select(x => x.Product)
            .ToList();
        var productsIds = products
            .Select(x => x.Id)
            .Distinct()
            .ToArray();
        
        // Preload attributes for all products in one go
        var batchContext = _productService.CreateProductBatchContext(products);
        await batchContext.Attributes.LoadAllAsync();
        var productSelectionMap = cart.Items.ToMultiMap(x => x.ProductId, x => x.AttributeSelection);
        await _attributeMaterializer.PrefetchProductVariantAttributeCombinationsAsync(productSelectionMap);
      
        // Preload medias for all products in one go
        var medias = await _db
            .ProductMedias.AsNoTracking()
            .Include(x => x.MediaFile)
            .Where(x => productsIds.Contains(x.ProductId) && x.MainImage)
            .ToListAsync();
        var productMediaMap = medias.ToDictionary(x => x.ProductId, x => x);
       
        
        var cartSubtotal = await _orderService.GetShoppingCartSubtotal(cart, batchContext);
        model.CartSubtotal = cartSubtotal;

        
        // Preload brands in one go
        if (settings.MapBrands)
        {
            await batchContext.ProductBrands.LoadAllAsync();
        }

        var context = new ShoppingCartModelContext()
        {
            Settings = settings,
            ShoppingCartSubtotal = cartSubtotal,
            Products = products,
            BatchContext = batchContext,
            ProductMediaMap = productMediaMap,
        };
        model.Items = await PrepareShoppingCartItemsAsync(cart.Items, context);
        return model;
    }


    protected virtual async Task<ICollection<ShoppingCartItemModel>> PrepareShoppingCartItemsAsync(
        IEnumerable<ShoppingCartItem> items, ShoppingCartModelContext ctx)
    {
        if (!items.Any())
            return [];

        var settings = ctx.Settings;
        List<ShoppingCartItemModel> modelList = new List<ShoppingCartItemModel>();
        foreach (var item in items)
        {
            var product = item.Product;
            var seName = await _urlService.GetActiveSlugAsync(product.Id, nameof(Product));
            var model = new ShoppingCartItemModel
            {
                Id = item.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                ProductUrl = _urlHelper.RouteUrl("Product", new { SeName = seName }),

                CurrentQuantity = item.Quantity,
                MaxAddToCartQuantity = product.MaxAddToCartNumber,
                MinAddToCartQuantity = product.MinAddToCartNumber,
            };

            if ( _attributeMaterializer.TryGetPrefetchedCombination(product.Id, item.AttributeSelection, out var selectedCombination))
            {
                model.MaxAddToCartQuantity =
                    (selectedCombination.StockQuantity != 0 &&
                     selectedCombination.StockQuantity > product.MaxAddToCartNumber)
                        ? product.MaxAddToCartNumber
                        : selectedCombination.StockQuantity;
            }
            
            if (settings.MapBrands)
            {
                var productBrand = (await ctx.BatchContext.ProductBrands.GetOrLoadAsync(product.Id)).FirstOrDefault();
                if (productBrand != null)
                {
                    model.Brand = await _catalogHelper.PrepareBrandSummaryModelAsync(productBrand.Brand);
                }
            }

            if (ctx.ShoppingCartSubtotal != null)
            {
                var cartLine = ctx.ShoppingCartSubtotal.ShoppingCartLines.FirstOrDefault(x =>
                    x.ShoppingCartItem.Id == item.Id);
                var priceModel = model.Price;
                priceModel.Subtotal = cartLine.Subtotal.FinalPrice;
                priceModel.UnitPrice = cartLine.UnitPrice.FinalPrice;
                if (cartLine.Subtotal.PriceSaving.HasSaving)
                {
                    // copy struct
                    priceModel.SubtotalSaving = cartLine.Subtotal.PriceSaving;
                }

                if (cartLine.UnitPrice.PriceSaving.HasSaving)
                {
                    priceModel.UnitSaving = cartLine.UnitPrice.PriceSaving;
                }
            }

            await MapAttributesAsync(model, item, ctx);
            await MapImageAsync(model, ctx);

            modelList.Add(model);
        }

        return modelList;
    }

    protected virtual async Task MapImageAsync(ShoppingCartItemModel model, ShoppingCartModelContext ctx)
    {
        //TODO: Do something to query a small copy of big files rather than simple let the browser query the widest image and resize it.
        Guard.NotNull(model);
        if (ctx.ProductMediaMap.TryGetValue(model.ProductId, out var mediaFile))
        {
            model.Image = (await _catalogHelper.PrepareProductImageModelAsync([mediaFile])).First();
        }
    }

    protected virtual async Task MapAttributesAsync(ShoppingCartItemModel model, ShoppingCartItem item,
        ShoppingCartModelContext ctx)
    {
        var attributes = await ctx.BatchContext.Attributes.GetOrLoadAsync(item.ProductId);
        var values =
            _attributeMaterializer.MaterializeProductVariantAttributeValues(item.AttributeSelection, attributes);
        model.AttributeValues = values;
    }

    public void GetBestFitShoppingCartModelMappingSettings
    (ShoppingCartModelMappingSettings settings, CartSummaryLocation location,
        Action<ShoppingCartModelMappingSettings> configuration = null)
    {
        settings ??= new ShoppingCartModelMappingSettings();
        if (location == CartSummaryLocation.ShoppingCart)
        {
            settings.MapBrands = true;
        }
        else if (location == CartSummaryLocation.CheckoutSummary)
        {
            settings.MapBrands = false;
        }
        else if (location == CartSummaryLocation.CartDrawer)
        {
            settings.MapBrands = false;
        }
    }
}

 

public class ShoppingCartModelMappingSettings
{
    public bool MapBrands { get; set; }
}

public enum CartSummaryLocation
{
    ShoppingCart,
    CheckoutSummary,
    CartDrawer
}

public class ShoppingCartModelContext
{
    public ShoppingCartModelMappingSettings Settings { get; set; }
    public ShoppingCartSubtotal ShoppingCartSubtotal { get; set; }
    public ICollection<Product> Products { get; set; } 
    public IDictionary<int, ProductMedia> ProductMediaMap { get; set; }
    public ProductBatchContext BatchContext { get; set; }
}