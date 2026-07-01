using EShop.Core.Catalog.Attributes.Domain;
using EShop.Web.Common.Models.Choices;

namespace EShop.Web.Models.Catalog;

public class ProductVariantAttributeModel : ChoiceModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsRequired { get; set; }
    public int TotalAttributeCount { get; set; }
    public AttributeControlType AttributeControlType { get; set; }
    public int ProductAttributeId { get; set; }
    public int ProductId { get; set; }
     

    public ProductVariantAttribute ProductVariantAttribute { get; set; }

    public override string BuildControlId()
    {
        return ProductVariantQueryItem.CreateKey(ProductId, ProductAttributeId, Id);
    }
}