using EShop.Core.Catalog.Attributes.Domain;
using EShop.Web.Common.Models.Choices;

namespace EShop.Web.Models.Catalog;

public class ProductVariantAttributeModel : ChoiceModel
{
    public AttributeControlType AttributeControlType { get; set; }

    public string Description { get; set; }

    public string TextValue { get; set; }

    public bool IsRequired { get; set; }

    public string Name { get; set; }
    
    public long ProductAttributeId { get; set; }

    public long ProductId { get; set; }
    public List<ChoiceItemModel> Values { get; set; }

    public ProductVariantAttribute ProductVariantAttribute { get; set; }
}