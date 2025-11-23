namespace EShop.Web.Models.Catalog;

public class ProductSpecificationModel
{
    public string SpecificationAttributeName { get; set; }
    public long SpecificationAttributeId { get; set; }
    public string SpecificationAttributeOption { get; set; }
    public int DisplayOrder { get; set; }

    public bool Essential { get; set; }
}