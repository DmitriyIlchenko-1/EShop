using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Content.Media.Domain;

public class ProductMedia : BaseEntity
{
    public byte DisplayOrder { get; set; }

    public Media Media { get; set; }

    public int MediaId { get; set; }

    public Product Product { get; set; }

    public int ProductId { get; set; }
}