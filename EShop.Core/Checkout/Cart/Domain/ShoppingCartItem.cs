using System.ComponentModel.DataAnnotations.Schema;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Data.Cart.Domain;

public class ShoppingCartItem : BaseEntity
{
    public int ShoppingCartId { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Quantity { get; set; }
    public string RawAttributes { get; set; }
    public Product Product { get; set; }
    
    private ProductVariantAttributeSelection _attributeSelection;

    [NotMapped]
    public ProductVariantAttributeSelection AttributeSelection
    {
        get => _attributeSelection ?? new ProductVariantAttributeSelection(RawAttributes);
    }

 
    public override int GetHashCode()
    {
        var combiner = HashCodeCombiner
            .Start()
            .Add(ProductId)
            .Add(Quantity)
            .Add(RawAttributes); 
        return combiner.GetCombinedHash();
    }
}