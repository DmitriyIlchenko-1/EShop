using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Checkout.Orders.Domain;
using EShop.Core.Common.Domain;
using EShop.Core.Data;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Data.Cart.Exceptions;
using EShop.Core.Data.Cart.Services;
using EShop.Core.Data.Orders.Exceptions;
using EShop.Core.Data.Payment.Exceptions;
using EShop.Core.Data.Payment.Services;
using EShop.Core.Data.Settings;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Extensions;
using EShop.Core.Platform.Logging.Services;
using EShop.Core.Platform.Modules;
using EShop.Core.Platform.Modules.Payment;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Checkout.Orders.Services;

public interface IOrderService
{
    Task<ShoppingCartSubtotal> GetShoppingCartSubtotal(ShoppingCart cart, ProductBatchContext batchContext = null,
        bool cache = true);

    Task<PlaceOrderResult> PlaceOrderAsync(ProcessPaymentRequest paymentRequest);
}

public class DefaultOrderService : IOrderService
{
    private readonly IProductPriceService _productPriceService;
    private readonly IProductService _productService;
    private readonly IRequestCache _requestCache;
    private readonly IWorkContext _workContext;
    private readonly CheckoutSettings _checkoutSettings;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IPaymentService _paymentService;
    private readonly ApplicationDbContext _db;
    private readonly IProductAttributeMaterializer _attributeMaterializer;


    public DefaultOrderService(IProductPriceService productPriceService, IProductService productService,
        IRequestCache requestCache, IWorkContext workContext, CheckoutSettings checkoutSettings,
        IShoppingCartService shoppingCartService, IPaymentService paymentService,
        ApplicationDbContext db, IProductAttributeMaterializer attributeMaterializer)
    {
        _productPriceService = productPriceService;
        _productService = productService;
        _requestCache = requestCache;
        _workContext = workContext;
        _checkoutSettings = checkoutSettings;
        _shoppingCartService = shoppingCartService;
        _paymentService = paymentService;
        _db = db;
        _attributeMaterializer = attributeMaterializer;
    }


    public virtual async Task<PlaceOrderResult> PlaceOrderAsync(ProcessPaymentRequest paymentRequest)
    {
        Guard.NotNull(paymentRequest);
        var placeOrderResult = new PlaceOrderResult();
        try
        {
            if (paymentRequest.OrderGuid == Guid.Empty)
            {
                //TODO: We've got to wrap this msg in an inner exception and display a user friendly msg instead.
                throw new OrderException("Order's Guid must not be empty in the passed PaymentRequestInfo");
            }

            OrderPlacementContext orderContext = new OrderPlacementContext();
            PrepareUserDetailsAsync(orderContext);
            await PrepareAndValidateShoppingCartAsync(orderContext);
            PrepareAndValidateShippingDetailsAsync(orderContext);
            await PrepareOrderTotalAsync(orderContext);
            var paymentResult = await ProcessPaymentAsync(paymentRequest);
            if (paymentResult.Succeeded)
            {
                var order = await SaveOrderAsync(paymentRequest, paymentResult, orderContext);
                await MigrateShoppingCartItemsToOrderAsync(order, orderContext);
                placeOrderResult.Order = order;
            }
            else
            {
                foreach (var error in paymentResult.Errors)
                {
                    placeOrderResult.Errors.Add(error);
                }
            }
        }
        catch (Exception e)
        {
            placeOrderResult.Errors.Add(e.Message);
        }

        return placeOrderResult;
    }


    protected virtual async Task<Order> SaveOrderAsync(ProcessPaymentRequest paymentRequest,
        ProcessPaymentResult paymentResult, OrderPlacementContext ctx)
    {
        var user = ctx.User;
        var order = new Order()
        {
            OrderGuid = paymentRequest.OrderGuid,
            UserId = user.Id,
            PaymentMethodSystemName = paymentRequest.PaymentMethodSystemName,
            Subtotal = ctx.CartSubtotal.SubtotalWithDiscount.Amount,
            SubtotalRounded = ctx.CartSubtotal.SubtotalWithDiscount.RoundedAmount,
            OrderDiscount = ctx.CartSubtotal.DiscountAmount,
            PaymentStatus = paymentResult.PaymentStatus,
            OrderStatus = OrderStatus.Pending,
            PaidOnUtc = null, // we look up if the order has been paid for later down the workflow.
            ShippingAddress = ctx.ShippingAddress
        };

        user.Orders.Add(order);
        // Save here to carry on with order item mapping.
        await _db.SaveChangesAsync();

        return order;
    }

    protected virtual async Task MigrateShoppingCartItemsToOrderAsync(Order order, OrderPlacementContext ctx)
    {
        foreach (var item in ctx.Cart.Items)
        {
            var product = item.Product;
            var lineSubtotal = ctx.CartSubtotal.ShoppingCartLines
                .FirstOrDefault(x => x.ShoppingCartItem.Id == item.Id);
            var orderItem = new OrderItem()
            {
                OrderItemGuid = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                RawAttributes = item.RawAttributes,
                Subtotal = lineSubtotal.Subtotal.FinalPrice,
                SubtotalRounded = lineSubtotal.Subtotal.FinalPrice.RoundedAmount,
                UnitPrice = lineSubtotal.UnitPrice.FinalPrice,
                UnitPriceRounded = lineSubtotal.UnitPrice.FinalPrice.RoundedAmount,
            };

            order.OrderItems.Add(orderItem);

            // Save here to safely adjust the product's stock
            await _db.SaveChangesAsync();

            await _productService.AdjustProductInventoryAsync(product, -item.Quantity, item.RawAttributes);

            var historyRecords = new List<DiscountUsageHistory>();
            foreach (var discount in ctx
                         .CartSubtotal.ShoppingCartLines
                         .Select(x => x.Subtotal.AppliedDiscount)
                         .Where(x => x != null))
            {
                historyRecords.Add(new DiscountUsageHistory()
                {
                    OrderId = order.Id,
                    DiscountId = discount.Id,
                    CreatedOnUtc = DateTime.UtcNow
                });
            }

            if (historyRecords.Any())
            {
                _db.DiscountUsageHistories.AddRange(historyRecords);
            }

            await _db.SaveChangesAsync();
        }

        await _shoppingCartService.ResetCartAsync(ctx.Cart);
    }

    protected virtual async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPayment)
    {
        return await _paymentService.ProcessPaymentAsync(processPayment);
    }

    protected virtual void PrepareUserDetailsAsync(OrderPlacementContext context)
    {
        context.User = _workContext.CurrentUser;
    }

    protected virtual void PrepareAndValidateShippingDetailsAsync(OrderPlacementContext context)
    {
        if (!context.User.ShippingAddressId.HasValue)
        {
            throw new ShoppingCartException("Shipping address must be specified to place order");
        }
        var shippingAddress = context.User.ShippingAddress;
        context.ShippingAddress = shippingAddress.Clone();
        context.ShippingStatus = ShippingStatus.NotShipped;
    }

    protected virtual async Task PrepareAndValidateShoppingCartAsync(OrderPlacementContext context)
    {
        var cart = await _shoppingCartService.GetUserCartAsync();
        context.Cart = cart;
        var productSelectionMap = cart.Items.ToMultiMap(x => x.ProductId, x=> x.AttributeSelection);
        await _attributeMaterializer.PrefetchProductVariantAttributeCombinationsAsync(productSelectionMap);
            
        if (!cart.Items.Any())
        {
            throw new ShoppingCartException("No items have been added to the cart to place the order");
        }

        var warnings = await _shoppingCartService.ValidateShoppingCartAsync(cart);
        if (warnings.Any())
        {
            throw new ShoppingCartException(string.Join(";", warnings));
        }

        foreach (var item in cart.Items)
        {
            var combination =  _attributeMaterializer.TryGetPrefetchedCombination(item.ProductId, item.AttributeSelection, out var prefetchedCombination) ? prefetchedCombination : null;
            warnings = await _shoppingCartService.ValidateShoppingCartItemAsync(item, combination);
            if (warnings.Any())
            {
                throw new ShoppingCartException(string.Join(";", warnings));
            }
        }
    }

    protected virtual async Task PrepareOrderTotalAsync(OrderPlacementContext context)
    {
        var subtotal = await GetShoppingCartSubtotal(context.Cart);
        context.CartSubtotal = subtotal;
    }

    public virtual async Task<ShoppingCartSubtotal> GetShoppingCartSubtotal(ShoppingCart cart,
        ProductBatchContext batchContext = null, bool cache = true)
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

        var subtotalWithDiscount = result
            .ShoppingCartLines.Select(x => x.Subtotal.FinalPrice.Amount)
            .Sum();
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

public class ProcessPaymentRequest
{
    public Guid OrderGuid { get; set; }
    public string PaymentMethodSystemName { get; set; }
}

public class PlaceOrderResult
{
    public bool Succeeded => !Errors.Any();
    public ICollection<string> Errors { get; } = [];
    public Order? Order { get; set; }
}

public class OrderPlacementContext
{
    public User User { get; set; }
    public ShoppingCart Cart { get; set; }
    public Address ShippingAddress { get; set; }
    public ShoppingCartSubtotal CartSubtotal { get; set; }
    public ShippingStatus ShippingStatus { get; set; }
}

public enum ShippingStatus
{
    NotShipped,
    Shipped
}