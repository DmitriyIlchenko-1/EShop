using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Checkout.Order.Domain;
using EShop.Core.Common.Domain;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Data.Cart.Services;
using EShop.Core.Data.Settings;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Extensions;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Data.Order.Services;

public interface IOrderService
{
    Task<ShoppingCartSubtotal> GetShoppingCartSubtotal(ShoppingCart cart, ProductBatchContext batchContext = null, bool cache = true);
}
public class DefaultOrderService : IOrderService
{
    private readonly IProductPriceService _productPriceService;
    private readonly IProductService _productService;
    private readonly IRequestCache _requestCache;
    private readonly IWorkContext _workContext;
    private readonly CheckoutSettings _checkoutSettings;
    private readonly IShoppingCartService _shoppingCartService;


    public DefaultOrderService(IProductPriceService productPriceService, IProductService productService, IRequestCache requestCache, IWorkContext workContext, CheckoutSettings checkoutSettings, IShoppingCartService shoppingCartService)
    {
        _productPriceService = productPriceService;
        _productService = productService;
        _requestCache = requestCache;
        _workContext = workContext;
        _checkoutSettings = checkoutSettings;
        _shoppingCartService = shoppingCartService;
    }
    
    

    public virtual async Task PlaceOrderAsync(PaymentRequestInfo paymentRequest)
    {
        Guard.NotNull(paymentRequest);
        if (paymentRequest.OrderGuid == Guid.Empty)
        {
            throw new InvalidOperationException("Order's Guid must not be empty in the passed PaymentRequestInfo");
        }

        OrderPlacementContext orderContext = new OrderPlacementContext();
        PrepareUserDetailsAsync(orderContext);
        await PrepareAndValidateShoppingCartAsync(orderContext);
      

    }

    protected virtual void PrepareUserDetailsAsync(OrderPlacementContext context)
    {
        context.User = _workContext.CurrentUser;
        if (!_checkoutSettings.AllowGuestsToOrder && context.User.IsGuest())
        {
           throw new InvalidOperationException("Orders cannot be placed by guests");
        }
    }

    protected virtual async Task PrepareAndValidateShippingDetailsAsync(OrderPlacementContext context)
    {
        if (!context.User.ShippingAddressId.HasValue)
        {
            throw new InvalidOperationException("Shipping address must be specified to place order");
        }
        
        
    }

    protected virtual async Task PrepareAndValidateShoppingCartAsync(OrderPlacementContext context)
    {
        var cart = await _shoppingCartService.GetUserCartAsync();
        context.Cart = cart;
        if (!cart.Items.Any())
        {
            throw new InvalidOperationException("No items have been added to the cart to place the order");
        }

        var warnings = await _shoppingCartService.ValidateShoppingCartAsync(cart);
        if (warnings.Any())
        {
            throw new InvalidOperationException(string.Join(";", warnings));
        }

        foreach (var item in cart.Items)
        {
            warnings = await _shoppingCartService.ValidateShoppingCartItemAsync(item);
            if (warnings.Any())
            {
                throw new InvalidOperationException(string.Join(";", warnings));
            }
        }

    }
     

    public virtual async Task<ShoppingCartSubtotal> GetShoppingCartSubtotal(ShoppingCart cart, ProductBatchContext batchContext = null, bool cache = true)
    {
        Guard.NotNull(cart);
        var hashCode = cart.GetHashCode();
        var cacheKey = $"cartcalculations:subtotal:{hashCode}";
        if (cache)
        {
            if (_requestCache.Contains(cacheKey))
            {
                return _requestCache.Get<ShoppingCartSubtotal>(cacheKey);
            }
        }
       
        batchContext ??= _productService.CreateProductBatchContext(cart.Items.Select(x => x.Product), false);
        ShoppingCartSubtotal result = new ShoppingCartSubtotal();
        var subtotalWithNoDiscount = 0m;
        foreach (var item in cart.Items)
        {
            var calculationContext = new PriceCalculationContext()
            {
                BatchContext = batchContext,
                Product = item.Product,
                Quantity = item.Quantity,
                CartItem = item
            };
            var (unitPrice, subtotal) = await _productPriceService.CalculateSubtotalAsync(calculationContext);
            subtotalWithNoDiscount += subtotal.RegularPrice.Amount;
            result.ShoppingCartLines.Add(new ShoppingCartItemLine(item)
            {
                Subtotal = subtotal,
                UnitPrice = unitPrice,
            });
        }
        var subtotalWithDiscount = result.ShoppingCartLines.Select(x => x.Subtotal.FinalPrice.Amount).Sum();
        result.SubtotalWithNoDiscount = new Money(subtotalWithNoDiscount);
        result.SubtotalWithDiscount = new Money(subtotalWithDiscount);
        if (cache)
        {
            _requestCache.Put(cacheKey, result);
        }
        return result;
    }
}


public class ShoppingCartSubtotalContext
{
    public ShoppingCartSubtotalContext(ShoppingCart cart)
    {
        ShoppingCart = cart;
    }
    public ShoppingCart ShoppingCart { get; set; }
    public ProductBatchContext BatchContext { get; set; }
}

public class PaymentRequestInfo
{
    
    public Guid OrderGuid { get; set; }
    public string PaymentMethodSystemName { get; set; }
}

public class OrderPlacementContext
{
    public User User { get; set; }
    public ShoppingCart Cart { get; set; }
    public Address ShippingAddress { get; set; }
}