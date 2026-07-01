using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Checkout.Order.Domain;
using EShop.Core.Data.Cart.Domain;
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


    public DefaultOrderService(IProductPriceService productPriceService, IProductService productService, IRequestCache requestCache)
    {
        _productPriceService = productPriceService;
        _productService = productService;
        _requestCache = requestCache;
    }

     

    public virtual async Task<ShoppingCartSubtotal> GetShoppingCartSubtotal(ShoppingCart cart, ProductBatchContext batchContext = null, bool cache = true)
    {
        Guard.NotNull(cart);
        var hashCode = cart.GetHashCode();
        var cacheKey = string.Format("cartcalculations:subtotal:{0}", hashCode);
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