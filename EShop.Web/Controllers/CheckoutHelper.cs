using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Checkout.Order.Domain;
using EShop.Core.Content.Media.Services;
using EShop.Core.Data;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Data.Order.Services;
using EShop.Core.Platform.Routing;
using EShop.Infrastructure.Utilities;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;

public class CheckoutHelper
{
    private readonly IUrlService _urlService;
    private readonly IUrlHelper _urlHelper;
    readonly CatalogHelper _catalogHelper;
    readonly ApplicationDbContext _db;
    readonly IBrandService _brandService;
    readonly IOrderService _orderService;
    readonly IProductService _productService;
    readonly IMediaService _mediaService;
    private readonly IProductAttributeMaterializer _attributeMaterializer;

    public CheckoutHelper(IUrlService urlService, IUrlHelper urlHelper, CatalogHelper catalogHelper,
        ApplicationDbContext db, IBrandService brandService, IOrderService orderService, IProductService productService,
        IMediaService mediaService, IProductAttributeMaterializer attributeMaterializer)
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
    }

    public async Task<ShoppingCartModel> PrepareShoppingCartModelAsync(ShoppingCart cart)
    {
        Guard.NotNull(cart);
        var model = new ShoppingCartModel();
        var products = cart
            .Items.Select(x => x.Product)
            .ToList();
        var batchContext = _productService.CreateProductBatchContext(products);
        await batchContext.Attributes.LoadAllAsync();
        var cartSubtotal = await _orderService.GetShoppingCartSubtotal(cart, batchContext);
        model.CartSubtotal = cartSubtotal;
        var brandIds =
            products
                .Select(p => p.BrandId ?? 0)
                .Where(x => x != 0)
                .Distinct()
                .ToArray();
      
        var brands = (await _brandService.GetBrandsByIdsAsync(brandIds)).ToDictionary(x => x.Id, x => x);
        var context = new ShoppingCartModelContext()
        {
            ShoppingCartSubtotal = cartSubtotal,
            Products = products,
            Brands = brands,
            BatchContext = batchContext
        };
        model.Items = await PrepareShoppingCartItemsAsync(cart.Items, context);
        return model;
    }


    protected virtual async Task<ICollection<ShoppingCartItemModel>> PrepareShoppingCartItemsAsync(
        IEnumerable<ShoppingCartItem> items, ShoppingCartModelContext ctx)
    {
        if (!items.Any())
            return [];

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
                MaxAddToCartQuantity = product.MaxAddToCartNumber
            };

            if (ctx.Brands.TryGetValue(product.BrandId ?? 0, out var brand) && brand != null)
            {
                model.Brand = await _catalogHelper.PrepareBrandSummaryModelAsync(brand);
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
            await MapImageAsync(model);
            
            modelList.Add(model);
        }

        return modelList;
    }

    protected virtual async Task MapImageAsync(ShoppingCartItemModel model)
    {
        //TODO: Do something to query a small copy of big files rather than simple let the browser query the widest image and resize it.
        Guard.NotNull(model);
        var mediaFile = await _db
            .ProductMedias.Include(x => x.MediaFile)
            .FirstOrDefaultAsync(x => x.ProductId == model.ProductId);
        if (mediaFile != null)
        {
            model.Image = (await _catalogHelper.PrepareProductImageModelAsync([mediaFile])).First();
        }
    }

    protected virtual async Task MapAttributesAsync(ShoppingCartItemModel model, ShoppingCartItem item, ShoppingCartModelContext ctx)
    {
        var attributes = await ctx.BatchContext.Attributes.GetOrLoadAsync(item.ProductId);
        var values = 
            _attributeMaterializer.MaterializeProductVariantAttributeValues(item.AttributeSelection, attributes);
        model.AttributeValues = values;
        
    }
}

public class ShoppingCartModelContext
{
    public ShoppingCartSubtotal ShoppingCartSubtotal { get; set; }
    public ICollection<Product> Products { get; set; }
    public IDictionary<int, Brand> Brands { get; set; }
    public ProductBatchContext BatchContext { get; set; }
}