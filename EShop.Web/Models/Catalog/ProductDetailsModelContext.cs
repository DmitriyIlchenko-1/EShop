using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;

namespace EShop.Web.Models.Catalog;

public class ProductDetailsModelContext  
{
    public ProductDetailsModelContext(Product product, ProductVariantQuery productVariantQuery,
        ProductBatchContext batchContext)
    {
        Product = product;
        ProductVariantQuery = productVariantQuery;
        BatchContext = batchContext;
    }

    public Product Product { get; set; }
    public ProductVariantQuery ProductVariantQuery { get; set; }
    public ProductBatchContext BatchContext { get; set; }
    public ProductVariantAttributeSelection Selection { get; set; }
     
}


 