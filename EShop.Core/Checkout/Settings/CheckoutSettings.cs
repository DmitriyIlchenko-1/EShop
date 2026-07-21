using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Data.Settings;

public class CheckoutSettings : ISettings
{
 
    public int MaxShoppingCartItems { get; set; } = 1_000;
    public bool AllowGuestsToOrder { get; set; } = true;
}