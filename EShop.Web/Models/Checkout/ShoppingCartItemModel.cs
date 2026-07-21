using EShop.Core.Catalog.Attributes.Domain;
using EShop.Web.Common.Models;
using EShop.Web.Models.Catalog;

namespace EShop.Web.Models.Checkout;

public class ShoppingCartItemModel : BaseModel
{
    public List<string> Warnings { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductUrl { get; set; }
    public ImageModel Image { get; set; }
    public int MaxAddToCartQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public BrandSummaryModel Brand { get; set; }
    public CartItemPriceModel Price { get; set; } = new();
    public ICollection<ProductVariantAttributeValue> AttributeValues { get; set; } = [];

}