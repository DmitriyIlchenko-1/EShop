using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data.Cart.Services;

public interface IShoppingCartService
{
    Task<(bool, ICollection<string>)> AddProductToCart(AddToCartContext ctx);
    Task<(bool, ICollection<string>)> UpdateCartItemAsync(ShoppingCartItem cartItem, int newQuantity);
     Task<int> GetUserCartItemCountAsync(User user = null);
     Task<ShoppingCart> GetUserCartAsync();

}
public class DefaultShoppingCartService : IShoppingCartService
{
    private readonly ApplicationDbContext _db;
    private readonly IProductAttributeMaterializer _attributeMaterializer;
    private readonly IWorkContext _workContext;
    readonly IRequestCache _requestCache;
    // 0 - user's id
    private const string ShoppingCartCacheKey = "shoppingcart:{0}";

    public DefaultShoppingCartService(ApplicationDbContext db, IWorkContext workContext, IProductAttributeMaterializer attributeMaterializer, IRequestCache requestCache)
    {
        _db = db;
        _workContext = workContext;
        _attributeMaterializer = attributeMaterializer;
        _requestCache = requestCache;
    }

    public virtual async Task<(bool, ICollection<string>)> AddProductToCart(AddToCartContext ctx)
    {
        Guard.NotNull(ctx);
        var quantity = ctx.Quantity;
        var query = ctx.VariantQuery;
        var product = ctx.Product;
        List<string> errors = new List<string>();
        if (quantity < 1 )
        {
            errors.Add("Quantity must be greater than 0");
            return (false, errors);
        }

        var userCart = await GetUserCartAsync();
        var selection =  _attributeMaterializer.CreateAttributeSelection(query, product.ProductVariantAttributes, product.Id);
        var rawAttributes = selection.AsJson();
        if (rawAttributes == null && product.AttributeCombinationRequired)
        {
            throw new InvalidOperationException(
                $"A combination is required to order {product.Id}:{product.Name} product");
        }
        var cartItem = userCart.Items.FirstOrDefault(x => x.ProductId == product.Id && x.RawAttributes == rawAttributes);
        if (cartItem == null)
        {
            cartItem = new ShoppingCartItem()
            {
                ProductId = ctx.Product.Id,
                ShoppingCartId = userCart.Id,
                RawAttributes = rawAttributes,
            };
            userCart.Items.Add(cartItem);
        }

        quantity = GetCorrectedNewQuantity(cartItem, product, quantity);
        if (quantity == 0)
        {
            errors.Add($"{product.MaxAddToCartNumber} is the maximum quantity you can add to your cart!");
            return (false, errors);
        }        
        
        cartItem.Quantity += quantity;
        

        await _db.SaveChangesAsync();
        return (true, errors);
    }

    public async Task<(bool, ICollection<string>)> UpdateCartItemAsync(ShoppingCartItem cartItem, int newQuantity)
    {
        Guard.NotNull(cartItem);
        var warnings = new List<string>();
        if (newQuantity < 0)
        {
            warnings.Add("Quantity must be greater than 0");
            return (false, warnings);
        }
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == cartItem.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException($"Cannot update cart info for non-existing product {cartItem.ProductId}.");
        }
        var userCart = await GetUserCartAsync();
        if (newQuantity == 0)
        {
            userCart.Items.Remove(cartItem);
        }
        else
        {
            if (product.MaxAddToCartNumber.HasValue && product.MaxAddToCartNumber.Value > newQuantity)
            {
                cartItem.Quantity = newQuantity;
            }
            else if (product.MaxAddToCartNumber.HasValue && product.MaxAddToCartNumber.Value <= newQuantity)
            {
                if (cartItem.Quantity != product.MaxAddToCartNumber.Value)
                {
                    cartItem.Quantity = product.MaxAddToCartNumber.Value;
                    warnings.Add($"{product.MaxAddToCartNumber.Value} has been added to your cart because of the limit.");
                }
            }
        }
         
        await _db.SaveChangesAsync();
        return (true, warnings);
    }

    protected virtual int GetCorrectedNewQuantity(ShoppingCartItem cartItem, Product product,  int newQuantity)
    {
        int canAddLeft = 0;
        if (product.MaxAddToCartNumber.HasValue)
        {
            if (cartItem.Quantity < product.MaxAddToCartNumber)
                canAddLeft = (int)product.MaxAddToCartNumber - cartItem.Quantity;
        }
        else
        {
                
            canAddLeft = int.MaxValue - cartItem.Quantity;
        }
        
        return canAddLeft > newQuantity ? newQuantity : canAddLeft;
    }


    public virtual async Task<int> GetUserCartItemCountAsync(User user = null)
    {
        user ??= _workContext.CurrentUser;
        var cacheKey = string.Format(ShoppingCartCacheKey, user.Id);
        if (_requestCache.TryGet(cacheKey, out ShoppingCart shoppingCart))
        {
            return shoppingCart.GetCount();
        }

        var cartItemCollection = _db
            .Entry(user)
            .Collection(x => x.ShoppingCartItems);
        if (!cartItemCollection.IsLoaded)
        {
            await cartItemCollection.LoadAsync();
        }
        return user.ShoppingCartItems.Select(x => x.Quantity).Sum();
    }

    public async Task<ShoppingCart> GetUserCartAsync()
    {
        var user = _workContext.CurrentUser;
        var cacheKey = string.Format(ShoppingCartCacheKey, user.Id);
        if (_requestCache.TryGet(cacheKey, out ShoppingCart shoppingCart))
        {
             return shoppingCart;
        }
        else
        {
            var userCart = await _db.ShoppingCarts
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (userCart is null)
            {
                userCart = new ShoppingCart() { UserId = user.Id, };
                _db.ShoppingCarts.Add(userCart);
                await _db.SaveChangesAsync();
            }
            
            _requestCache.Put(cacheKey, userCart);
            return userCart;
        }
        
       
    }
}


public class AddToCartContext
{
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public ProductVariantQuery VariantQuery { get; set; }
}