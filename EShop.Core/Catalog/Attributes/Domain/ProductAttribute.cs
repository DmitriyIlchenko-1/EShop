using EShop.Infrastructure.Domain;

namespace EShop.Core.Catalog.Attributes.Domain;

/// <summary>
/// Represents the table for product attributes like Color, Length, Material etc. 
/// Represents 'one' end in One-To-Many  relationship with <see cref="ProductAttributeOptionsSets"/>. 
/// </summary>
public class ProductAttribute : BaseEntity
{
    public string Alias { get; set; }

    public string Description { get; set; }

    public int DisplayOrder { get; set; }
    public string TextPrompt { get; set; }

    public string Name { get; set; }

    public ICollection<ProductAttributeOptionsSet> ProductAttributeOptionsSets { get; set; }
}