using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Catalog.Brands.Domain;

public class ProductBrand : BaseEntity, IDisplayOrder
{
    public int ProductId { get; set; }
    public int BrandId { get; set; }
    public Product Product { get; set; }
    public Brand Brand { get; set; }
    public int DisplayOrder { get; set; }
}