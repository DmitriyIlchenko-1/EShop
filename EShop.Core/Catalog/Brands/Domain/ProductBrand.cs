using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Catalog.Brands.Domain;


/// <summary>
/// This table exists for the sake of being able to create 'maps' while mapping domain models to DTOs. We need to have a product id field to create dictionaries / multimaps as in <see cref="ProductBatchContext"/>
/// </summary>
public class ProductBrand : BaseEntity
{
    public int DisplayOrder { get; set; }
    public int BrandId { get; set; }
    public Brand Brand { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
}