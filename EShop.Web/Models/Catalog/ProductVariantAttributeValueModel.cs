using EShop.Core.Catalog.Attributes.Domain;
using EShop.Web.Common.Models.Choices;

namespace EShop.Web.Models.Catalog;

public class ProductVariantAttributeValueModel : ChoiceItemModel
{
    public ProductVariantAttributeValue ProductVariantAttributeValue { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsEssential { get; set; }
}