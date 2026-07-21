using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Data.Settings;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data.Cart.Services;

public interface IShoppingCartService
{
    Task<ICollection<string>> AddProductToCart(AddToCartContext ctx);
    Task<ICollection<string>> UpdateCartItemAsync(ShoppingCartItem cartItem, int newQuantity);
    Task RemoveCartItemAsync(ShoppingCartItem cartItem);
    Task<int> GetUserCartItemCountAsync(User user = null);
    Task<ShoppingCart> GetUserCartAsync(User? user = null);
    Task<ICollection<string>> ValidateShoppingCartAsync(ShoppingCart shoppingCart);
    Task<ICollection<string>> ValidateShoppingCartItemAsync(ShoppingCartItem shoppingCartItem);
}

public class DefaultShoppingCartService : IShoppingCartService
{
    private readonly ApplicationDbContext _db;
    private readonly IProductAttributeMaterializer _attributeMaterializer;
    private readonly IWorkContext _workContext;
    readonly IRequestCache _requestCache;
    private readonly CheckoutSettings _settings;

    private readonly IProductAttributeMaterializer _productAttributeMaterializer;

    // 0 - user's id
    private const string ShoppingCartCacheKey = "shoppingcart:{0}";

    public DefaultShoppingCartService(ApplicationDbContext db, IWorkContext workContext,
        IProductAttributeMaterializer attributeMaterializer, IRequestCache requestCache, CheckoutSettings settings,
        IProductAttributeMaterializer productAttributeMaterializer)
    {
        _db = db;
        _workContext = workContext;
        _attributeMaterializer = attributeMaterializer;
        _requestCache = requestCache;
        _settings = settings;
        _productAttributeMaterializer = productAttributeMaterializer;
    }

    public virtual async Task<ICollection<string>> AddProductToCart(AddToCartContext ctx)
    {
        Guard.NotNull(ctx);
        var quantity = ctx.Quantity;
        var query = ctx.VariantQuery;
        var product = ctx.Product;
        List<string> warnings = new List<string>();
        if (quantity < 1)
        {
            warnings.Add("Quantity must be greater than 0");
            return warnings;
        }

        var userCart = await GetUserCartAsync();
        var selection =
            _attributeMaterializer.CreateAttributeSelection(query, product.ProductVariantAttributes, product.Id);
        var rawAttributes = selection.AsJson();
        if (rawAttributes == null && product.AttributeCombinationRequired)
        {
            throw new InvalidOperationException(
                $"A combination is required to order {product.Id}:{product.Name} product");
        }

        var cartItem =
            userCart.Items.FirstOrDefault(x => x.ProductId == product.Id && x.RawAttributes == rawAttributes);
        if (cartItem == null)
        {
            cartItem = new ShoppingCartItem()
            {
                UserId = userCart.User.Id,
                ProductId = ctx.Product.Id,
                RawAttributes = rawAttributes,
                AddedOnUtc = DateTime.UtcNow,
            };

            userCart.User.ShoppingCartItems.Add(cartItem);
        }

        var cacheKey = string.Format(ShoppingCartCacheKey, userCart.User.Id);
        _requestCache.Remove(cacheKey);

        int canAddLeft = 0;
        // if (product.MaxAddToCartNumber.HasValue)
        // {
        //     if (cartItem.Quantity < product.MaxAddToCartNumber)
        //         canAddLeft = (int)product.MaxAddToCartNumber - cartItem.Quantity;
        // }
        // else
        // {
        //     canAddLeft = _settings.MaxAddToCartNumber - cartItem.Quantity;
        // }

        int correctedQuantity = canAddLeft > quantity ? quantity : canAddLeft;
        // if (canAddLeft < quantity)
        // {
        //     var limitStr = canAddLeft == 0 ? "None" : correctedQuantity.ToString();
        //     warnings.Add(
        //         $"{limitStr} has been added to your cart because of the limit: {product.MaxAddToCartNumber ?? _settings.MaxAddToCartNumber}");
        // }

        cartItem.Quantity += correctedQuantity;


        await _db.SaveChangesAsync();
        return warnings;
    }

    public async Task<ICollection<string>> UpdateCartItemAsync(ShoppingCartItem cartItem, int newQuantity)
    {
        Guard.NotNull(cartItem);
        var warnings = new List<string>();
        if (newQuantity < 0)
        {
            warnings.Add("Quantity must be greater than 0");
            return warnings;
        }

        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == cartItem.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException(
                $"Cannot update cart info for non-existing product {cartItem.ProductId}.");
        }

        // if (product.MaxAddToCartNumber.HasValue && product.MaxAddToCartNumber.Value >= newQuantity)
        // {
        //     cartItem.Quantity = newQuantity;
        // }
        // else if (product.MaxAddToCartNumber.HasValue && product.MaxAddToCartNumber.Value < newQuantity)
        // {
        //     if (cartItem.Quantity <= product.MaxAddToCartNumber.Value &&
        //         product.MaxAddToCartNumber.Value <= newQuantity)
        //     {
        //         warnings.Add($"{product.MaxAddToCartNumber.Value} has been added to your cart because of the limit.");
        //     }
        //
        //     cartItem.Quantity = product.MaxAddToCartNumber.Value;
        // }

        await _db.SaveChangesAsync();
        return warnings;
    }

    public async Task RemoveCartItemAsync(ShoppingCartItem cartItem)
    {
        Guard.NotNull(cartItem);
        _db.ShoppingCartItems.Remove(cartItem);
        await _db.SaveChangesAsync();
    }


    public virtual async Task<int> GetUserCartItemCountAsync(User user = null)
    {
        user ??= _workContext.CurrentUser;
        var cacheKey = string.Format(ShoppingCartCacheKey, user.Id);
        if (_requestCache.TryGet(cacheKey, out ShoppingCart shoppingCart))
        {
            return shoppingCart.GetCount();
        }

        await LoadShoppingCartItemsAsync(user, true);
        return user
            .ShoppingCartItems.Select(x => x.Quantity)
            .Sum();
    }

    public async Task<ShoppingCart> GetUserCartAsync(User user = null)
    {
        user ??= _workContext.CurrentUser;
        var cacheKey = string.Format(ShoppingCartCacheKey, user.Id);
        if (_requestCache.TryGet(cacheKey, out ShoppingCart shoppingCart))
        {
            return shoppingCart;
        }

        await LoadShoppingCartItemsAsync(user, true);
        var cart = new ShoppingCart(user, user.ShoppingCartItems);
        _requestCache.Put(cacheKey, cart);
        return cart;
    }

    public virtual Task<ICollection<string>> ValidateShoppingCartAsync(ShoppingCart shoppingCart)
    {
        Guard.NotNull(shoppingCart);
         var warnings = new List<string>();
        // if (shoppingCart.GetCount() > _settings.MaxAddToCartNumber)
        // {
        //     warnings.Add($"The cart contains more cart items than allowed by limit {_settings.MaxAddToCartNumber}");
        // }

        return Task.FromResult<ICollection<string>>(warnings);
    }

    public virtual async Task<ICollection<string>> ValidateShoppingCartItemAsync(ShoppingCartItem shoppingCartItem)
    {
        Guard.NotNull(shoppingCartItem);
        var warnings = new List<string>();
        var product = shoppingCartItem.Product;
        if (product.IsDeleted)
        {
            warnings.Add($"Cart contains a deleted product {product.Id}:{product.Name}");
        }

        if (product.IsPublished)
        {
            warnings.Add($"Cart contains a non published product {product.Id}:{product.Name}");
        }

        if (shoppingCartItem.Quantity < product.MinAddToCartNumber)
        {
            warnings.Add(
                $"Product's ({product.Id}:{product.Name}) quantity in cart must be greater than or equal to {product.MinAddToCartNumber}");
        }

        if (shoppingCartItem.Quantity > product.MaxAddToCartNumber)
        {
            warnings.Add(
                $"Product's ({product.Id}:{product.Name}) quantity in cart must be smaller than {product.MaxAddToCartNumber}");
        }

        //TODO: Have to implement stock management first, for the time being we treat products as though they can't be ordered without a combination.
        // if (shoppingCartItem.Quantity > product.StockQuantity)
        // {
        //     warnings.Add($"Product's ({product.Id}:{product.Name}) stock quantity cannot be smaller than the cart item's quantity");
        // }

        var combination =
            await _productAttributeMaterializer.FindAttributeCombinationAsync(product.Id,
                shoppingCartItem.AttributeSelection);
        if (combination != null)
        {
            if (!combination.IsActive)
            {
                warnings.Add(
                    $"Cannot add inactive product combination. combination id:{combination.Id}, product id: {product.Id}");
            }
            else if (shoppingCartItem.Quantity > combination.StockQuantity)
            {
                warnings.Add(
                    $"Cart item's {shoppingCartItem.Id} quantity cannot be greater than its product's selected combination's stock quantity.");
            }
        }


        return warnings;
    }


    protected async Task LoadShoppingCartItemsAsync(User user, bool force = false)
    {
        await _db.LoadCollectionAsync(user,
            x => x.ShoppingCartItems,
            force,
            x =>
            {
                return x
                    .OrderByDescending(x => x.AddedOnUtc)
                    .Include(x => x.Product);
            });
    }
}

public class AddToCartContext
{
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public ProductVariantQuery VariantQuery { get; set; }
}