using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Price;
using EShop.Web.Common.Models;
using EShop.Web.Models.Catalog;
using EShop.Web.Models.Checkout;

namespace EShop.Web.Models.Account;

public class OrderItemModel : BaseModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductUrl { get; set; }
    public ImageModel Image { get; set; }
    public int MaxAddToCartQuantity { get; set; }
    public int Quantity { get; set; }
    public BrandSummaryModel Brand { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public ICollection<ProductVariantAttributeValue> AttributeValues { get; set; } = [];
}

 