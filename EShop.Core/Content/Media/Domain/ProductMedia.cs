using System.ComponentModel.DataAnnotations.Schema;
using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Content.Media.Domain;

[Table("Content_ProductMedia_Mapping")]
public class ProductMedia : BaseEntity
{
    public byte DisplayOrder { get; set; }

    public MediaFile MediaFile { get; set; }

    public int MediaId { get; set; }

    public Product Product { get; set; }

    public int ProductId { get; set; }
}