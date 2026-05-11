using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Catalog.Attributes.Domain;

/// <summary>
/// Represents a specification attribute that can be assigned to a <see cref="Product"/> entity
/// </summary>
public class SpecificationAttribute : BaseEntity
{
    public string Alias { get; set; }

    public bool AllowFiltering { get; set; }

    public int DisplayOrder { get; set; }

    public string Name { get; set; }

    

    public ICollection<SpecificationAttributeOption> SpecificationAttributeOptions { get; set; }
}