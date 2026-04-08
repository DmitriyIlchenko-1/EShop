namespace EShop.Core.Catalog.Attributes.Domain;

public class ProductVariantQueryItem
{
    public static string CreateKey(int productId, int attributeId, int variantAttributeId)
        => $"pvatr{productId}-{attributeId}-{variantAttributeId}";

    public int ProductId { get; set; }
    public int AttributeId { get; set; }
    public int VariantAttributeId { get; set; }
    public string Value { get; set; }
    
}