using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Data.Cart.Domain;

public class ShoppingCart 
{
    public ShoppingCart(User user, IEnumerable<ShoppingCartItem> items)
    {
        Guard.NotNull(user);
        Guard.NotNull(items);
        User = user;
        Items = items.ToList();
    }
    public User User { get; set; }
    public ICollection<ShoppingCartItem> Items { get; set; }

    public int GetCount()
    {
        return Items.Select(x => x.Quantity).Sum();
    }
   
    public override int GetHashCode()
    {
        return HashCodeCombiner
            .Start()
            .Add(User.Id)
            .AddRange(Items)
            .GetCombinedHash();
    }
}