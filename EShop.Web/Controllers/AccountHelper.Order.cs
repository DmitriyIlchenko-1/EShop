using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Checkout.Orders.Domain;
using EShop.Core.Common.Services;
using EShop.Core.Content.Media.Domain;
using EShop.Core.Data;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Modules;
using EShop.Core.Platform.Routing;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using EShop.Web.Models.Account;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;

public partial class AccountHelper
{
    private readonly IWorkContext _workContext;
    private readonly ApplicationDbContext _db;
    private readonly IDateTimeService _dateTimeService;
    private readonly IPaymentProviderManager _paymentProviderManager;
    private readonly IUrlHelper _urlHelper;
    private readonly IUrlService _urlService;
    private readonly PerformanceSettings _performanceSettings;
    private readonly IProductAttributeMaterializer _attributeMaterializer;
    private readonly IProductService _productService;
    private readonly CatalogHelper _catalogHelper;
    private readonly ICityService _cityService;
    public AccountHelper(IWorkContext workContext, IDateTimeService dateTimeService, ApplicationDbContext db, IPaymentProviderManager paymentProviderManager, IUrlHelper urlHelper, IUrlService urlService, PerformanceSettings performanceSettings, IProductAttributeMaterializer attributeMaterializer, IProductService productService, CatalogHelper catalogHelper, ICityService cityService)
    {
        _workContext = workContext;
        _dateTimeService = dateTimeService;
        _db = db;
        _paymentProviderManager = paymentProviderManager;
        _urlHelper = urlHelper;
        _urlService = urlService;
        _performanceSettings = performanceSettings;
        _attributeMaterializer = attributeMaterializer;
        _productService = productService;
        _catalogHelper = catalogHelper;
        _cityService = cityService;
    }

    public virtual async Task<OrderListModel> PrepareOrderListModelAsync()
    {
        var user = _workContext.CurrentUser;
        var model = new OrderListModel();
        var orders = await _db
            .Orders
            .Where(x => x.UserId == user.Id)
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .ToListAsync();
        foreach (var order in orders)
        {
            var orderModel = PrepareOrderModelAsync(order);
            model.Orders.Add(orderModel);
        }

        return model;
    }

    public virtual async Task<OrderDetailModel> PrepareOrderDetailModelAsync(Order order)
    {
        Guard.NotNull(order);
        var model = new OrderDetailModel
        {
            Id = order.Id,
            OrderDate = _dateTimeService.ConvertToLocalTimeZoneFromUtc(order.CreatedOnUtc),
            OrderStatus = order.OrderStatus.ToString(),
            Subtotal = order.SubtotalRounded,
            PaymentMethodName = order.PaymentMethodSystemName,
            AddressModel = new AddressModel()
            {
                Id = order.ShippingAddress.Id,
                AddressLine1 = order.ShippingAddress.AddressLine1,
                AddressLine2 = order.ShippingAddress.AddressLine2,
                CityName = order.ShippingAddress.CityId.HasValue ? (await _cityService.GetByIdAsync(order.ShippingAddress.CityId.Value, true)).Name 
                    : string.Empty,
                FirstName = order.ShippingAddress.FirstName,
                LastName = order.ShippingAddress.LastName,
                PhoneNumber = order.ShippingAddress.PhoneNumber,
                ZipCode = order.ShippingAddress.ZipCode,
            }
        };
        var context = new OrderDetailModelContext();
        
        var products = order
            .OrderItems.Select(x => x.Product)
            .ToList();
        var productsIds = products
            .Select(x => x.Id)
            .Distinct()
            .ToArray();
        var medias = await _db
            .ProductMedias.AsNoTracking()
            .Include(x => x.MediaFile)
            .Where(x => productsIds.Contains(x.ProductId) && x.MainImage)
            .ToListAsync();
        context.MediaMap = medias.ToDictionary(x => x.ProductId, x => x);
            
        var paymentProvider = _paymentProviderManager.GetActivePaymentMethod(order.PaymentMethodSystemName);
        model.PaymentMethodName = paymentProvider != null ? paymentProvider.Metadata.FriendlyName : order.PaymentMethodSystemName;
      
        if (_performanceSettings.AlwaysPrefetchUrlSlugs)
        {
            var productIds = products.Select(x => x.Id);
            await _urlService.PrefetchUrlRecordsAsync(nameof(Product), productIds);
        }
       
        var batchContext = context.BatchContext = _productService.CreateProductBatchContext(products);
        await batchContext.Attributes.LoadAllAsync();
        await batchContext.ProductBrands.LoadAllAsync();
        foreach (var item in order.OrderItems)
        {
            model.OrderItems.Add(await PrepareOrderItemModel(item, context));
        }

        return model;
    }
    
    protected virtual OrderModel PrepareOrderModelAsync(Order order)
    {
        var model = new OrderModel();
        model.Id = order.Id;
        model.OrderStatus = order.OrderStatus.ToString();
        model.OrderDate = _dateTimeService.ConvertToLocalTimeZoneFromUtc(order.CreatedOnUtc);
        model.Subtotal = order.SubtotalRounded;
        return model;
    }

    protected virtual async Task<OrderItemModel> PrepareOrderItemModel(OrderItem item, OrderDetailModelContext ctx)
    {
        var batchContext = ctx.BatchContext;
        var product = item.Product;
        var seName = await _urlService.GetActiveSlugAsync(product.Id, product.GetEntityName());
        var model = new OrderItemModel
        {
            ProductId = item.ProductId,
            ProductName = item.Product.Name,
            ProductUrl = _urlHelper.RouteUrl("Product", new { SeName = seName }),
            Quantity = item.Quantity,
            UnitPrice = item.UnitPriceRounded,
            Subtotal = item.SubtotalRounded
        };
        var selection = new ProductVariantAttributeSelection(item.RawAttributes);
        var attributes = await batchContext.Attributes.GetOrLoadAsync(product.Id);
        var attributeValues = _attributeMaterializer.MaterializeProductVariantAttributeValues(selection, attributes);
        model.AttributeValues = attributeValues;
        await MapImageAsync(model, ctx);
        //TODO: What if productBrand is null? What do we do?  The same goes for where this mapping takes place in other helpers like CatalogHelper.Summary or ShoppingCartHelper.
        var productBrand = (await batchContext.ProductBrands.GetOrLoadAsync(product.Id)).FirstOrDefault();
        if (productBrand != null)
        {
            model.Brand = await _catalogHelper.PrepareBrandSummaryModelAsync(productBrand.Brand);
        }
        return model;
    }
    
    protected virtual async Task MapImageAsync(OrderItemModel model, OrderDetailModelContext ctx)
    {
        var mediaMap = ctx.MediaMap;
        //TODO: Do something to query a small copy of big files rather than simple let the browser query the widest image and resize it.
        Guard.NotNull(model);
        if (mediaMap.TryGetValue(model.ProductId, out var mediaFile))
        {
            model.Image = (await _catalogHelper.PrepareProductImageModelAsync([mediaFile])).First();
        }
    }

    public class OrderDetailModelContext
    {
        public IDictionary<int, ProductMedia> MediaMap { get; set; }
        public ProductBatchContext BatchContext { get; set; }   
    }
}

