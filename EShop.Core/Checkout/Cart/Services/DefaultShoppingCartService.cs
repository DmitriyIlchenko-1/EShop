using EShop.Caching;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Data.Settings;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data.Cart.Services;

public interface IShoppingCartService
{
    Task<(ICollection<string> Warning, bool IsAdded)> AddProductToCart(AddToCartContext ctx);
    Task<ICollection<string>> UpdateCartItemAsync(ShoppingCartItem item, int newQuantity);
    Task ResetCartAsync(ShoppingCart cart);
    Task RemoveCartItemAsync(ShoppingCartItem cartItem);
    Task<int> GetUserCartItemCountAsync(User user = null);
    Task<ShoppingCart> GetUserCartAsync(User? user = null);
    Task<bool> MigrateCartAsync(User migrateFrom, User migrateTo);
    Task<ICollection<string>> ValidateShoppingCartAsync(ShoppingCart shoppingCart);

    Task<ICollection<string>> ValidateShoppingCartItemAsync(ShoppingCartItem shoppingCartItem,
        ProductVariantAttributeCombination combination);
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
    private const string ShoppingCartKeyPattern = "shoppingcart:*";


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

    public virtual async Task<(ICollection<string>, bool)> AddProductToCart(AddToCartContext ctx)
    {
        Guard.NotNull(ctx);
        var warnings = new List<string>();
        var quantity = ctx.Quantity;
        var query = ctx.VariantQuery;
        var product = ctx.Product;
        var user = ctx.User ?? _workContext.CurrentUser;
        var userCart = await GetUserCartAsync(user);
        var errors = await ValidateShoppingCartAsync(userCart);
        if (quantity < 1)
        {
            warnings.Add("Quantity must be greater than 0");
            return (warnings, false);
        }

        if (errors.Any())
        {
            return (errors, false);
        }

       
        var selection =
           ctx.Selection ?? _attributeMaterializer.CreateAttributeSelection(query, product.ProductVariantAttributes, product.Id);

        if (selection.IsEmpty() && (product.AttributeCombinationRequired ||
                                    product.ProductVariantAttributes.Any(x => x.IsRequired)))
        {
            warnings.Add($"Choose from the given options to order {product.Name}");
            return (warnings, false);
        }

        var rawAttributes = selection.AsJson();
        var item = userCart.Items.FirstOrDefault(x =>
            x.ProductId == product.Id && (!product.AttributeCombinationRequired || x.RawAttributes == rawAttributes));
        if (item == null)
        {
            item = new ShoppingCartItem()
            {
                UserId = userCart.User.Id,
                ProductId = ctx.Product.Id,
                RawAttributes = rawAttributes,
                AddedOnUtc = DateTime.UtcNow,
                Product = product,
            };
        }

        int stockQuantity = product.StockQuantity;
        var combination = await _attributeMaterializer.FindAttributeCombinationAsync(item.ProductId, item.AttributeSelection);
        if (product.AttributeCombinationRequired)
        {
            stockQuantity = combination.StockQuantity;
        }


        var maxAddToCartNumber =
            (stockQuantity != 0 && stockQuantity > product.MaxAddToCartNumber)
                ? product.MaxAddToCartNumber
                : stockQuantity;

        if (!userCart.Items.Contains(item) && quantity < product.MinAddToCartNumber)
        {
            item.Quantity = product.MinAddToCartNumber;

            warnings.Add(
                $"{product.MinAddToCartNumber} has been added to your cart because of the min. limit of {product.MinAddToCartNumber}");
        }
        else
        {
            int canAddLeft = 0;
            if (item.Quantity < maxAddToCartNumber)
            {
                canAddLeft = maxAddToCartNumber - item.Quantity;
            }

            int correctedQuantity = canAddLeft > quantity ? quantity : canAddLeft;
            if (canAddLeft < quantity)
            {
                var limitStr = canAddLeft == 0 ? "None" : correctedQuantity.ToString();
                warnings.Add(
                    $"{limitStr} has been added to your cart because of the limit: {maxAddToCartNumber}");
            }

            item.Quantity += correctedQuantity;
        }


        // combination can be null at this point if product doesn't have any
        errors = await ValidateShoppingCartItemAsync(item, combination);
        if (errors.Any())
        {
            return (errors, false);
        }

        var cacheKey = string.Format(ShoppingCartCacheKey, userCart.User.Id);
        _requestCache.Remove(cacheKey);

        if (!userCart.User.ShoppingCartItems.Contains(item))
        {
            userCart.User.ShoppingCartItems.Add(item);
        }

        await _db.SaveChangesAsync();
        return (warnings, true);
    }


    public async Task<ICollection<string>> UpdateCartItemAsync(ShoppingCartItem item, int newQuantity)
    {
        Guard.NotNull(item);
        var userCart = await GetUserCartAsync();
        var warnings = new List<string>();
        var errors = await ValidateShoppingCartAsync(userCart);
        if (newQuantity < 1)
        {
            return ["Quantity must be greater than 0"];
        }

        if (errors.Any())
        {
            return errors;
        }

        var product = item.Product;
        if (product == null)
        {
            throw new InvalidOperationException(
                $"Cannot update cart info for non-existing product {item.ProductId}.");
        }

        int stockQuantity = product.StockQuantity;
        var combination =
            await _attributeMaterializer.FindAttributeCombinationAsync(item.ProductId, item.AttributeSelection);
        if (product.AttributeCombinationRequired)
        {
            stockQuantity = combination.StockQuantity;
        }


        var maxAddToCartNumber =
            (stockQuantity != 0 && stockQuantity > product.MaxAddToCartNumber)
                ? product.MaxAddToCartNumber
                : stockQuantity;

        if (newQuantity < product.MinAddToCartNumber)
        {
            item.Quantity = product.MinAddToCartNumber;
            warnings.Add(
                $"{product.MinAddToCartNumber} has been added to your cart because of the min. limit of {product.MinAddToCartNumber}");
        }
        else
        {
            if (maxAddToCartNumber >= newQuantity)
            {
                item.Quantity = newQuantity;
            }
            else if (maxAddToCartNumber < newQuantity)
            {
                if (item.Quantity <= maxAddToCartNumber &&
                    maxAddToCartNumber <= newQuantity)
                {
                    warnings.Add($"{maxAddToCartNumber} has been added to your cart. You can't add more");
                }

                item.Quantity = maxAddToCartNumber;
            }
        }


        errors = await ValidateShoppingCartItemAsync(item, combination);
        if (errors.Any())
        {
            return errors;
        }

        await _db.SaveChangesAsync();
        return warnings;
    }

    public async Task ResetCartAsync(ShoppingCart cart)
    {
        Guard.NotNull(cart);
        _db.ShoppingCartItems.RemoveRange(cart.Items);
        cart.Items.Clear();
        await _db.SaveChangesAsync();
        _requestCache.RemoveByPattern(ShoppingCartKeyPattern);
    }

    public async Task RemoveCartItemAsync(ShoppingCartItem cartItem)
    {
        Guard.NotNull(cartItem);
        var userCart = await GetUserCartAsync();
        userCart.Items.Remove(cartItem);
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

        await LoadShoppingCartItemsAsync(user);
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

        await LoadShoppingCartItemsAsync(user);
        var cart = new ShoppingCart(user, user.ShoppingCartItems);
        _requestCache.Put(cacheKey, cart);
        return cart;
    }

    public async Task<bool> MigrateCartAsync(User migrateFrom, User migrateTo)
    {
        Guard.NotNull(migrateFrom);
        Guard.NotNull(migrateTo);
        var migrateFromCart = await GetUserCartAsync(migrateFrom);
        if (migrateFromCart.GetCount() == 0)
        {
            return false;
        }
        
        foreach (var item in migrateFromCart.Items)
        {
            var copiedItem = new AddToCartContext(item)
            {
                User = migrateTo
            };
            await AddProductToCart(copiedItem);
        }

        await ResetCartAsync(migrateFromCart);

        return true;
    }

    public virtual Task<ICollection<string>> ValidateShoppingCartAsync(ShoppingCart shoppingCart)
    {
        Guard.NotNull(shoppingCart);
        var warnings = new List<string>();
        if (shoppingCart.GetCount() > _settings.MaxShoppingCartItems)
        {
            warnings.Add($"The cart contains more cart items than allowed by limit {_settings.MaxShoppingCartItems}");
        }

        return Task.FromResult<ICollection<string>>(warnings);
    }

    public virtual async Task<ICollection<string>> ValidateShoppingCartItemAsync(
        ShoppingCartItem item, ProductVariantAttributeCombination? combination = null)
    {
        Guard.NotNull(item);
        var errors = new List<string>();
        var product = item.Product;
        if (product.IsDeleted)
        {
            errors.Add($"Cart contains a deleted product");
        }

        if (!product.IsPublished)
        {
            errors.Add($"The product you're trying to add is not published");
        }

        // These two checks are redundant because we safely fix any logical errors before we call this method.
        // if (item.Quantity < product.MinAddToCartNumber)
        // {
        //     errors.Add(
        //         $"Product's ({product.Id}:{product.Name}) quantity in cart must be greater than or equal to {product.MinAddToCartNumber}");
        // }
        //
        // if (item.Quantity > product.MaxAddToCartNumber)
        // {
        //     errors.Add(
        //         $"Product's ({product.Id}:{product.Name}) quantity in cart must be smaller than {product.MaxAddToCartNumber}");
        // }

        if (combination != null)
        {
            if (!combination.IsActive)
            {
                errors.Add($"Selected product's combination is not available.");
            }
        }
        else
        {
            if (item.Quantity > product.StockQuantity)
            {
                errors.Add($"Stock quantity is not big enough to reach the minimum order amount for this product");
            }
        }

        return errors;
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
    public AddToCartContext(ShoppingCartItem item)
    {
        Product = item.Product;
        Quantity = item.Quantity;
        Selection = item.AttributeSelection;
        VariantQuery =  ProductVariantQuery.Empty;
    }

    public AddToCartContext()
    {
    }

    public User User { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public ProductVariantQuery VariantQuery { get; set; }
    public ProductVariantAttributeSelection Selection { get; set; }
}