using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;

namespace EShop.Web.Models.Catalog;

public class ProductDetailsModelContext  
{
    public ProductDetailsModelContext(Product product, ProductVariantQuery productVariantQuery,
        ProductLazyContext lazyContext)
    {
        Product = product;
        ProductVariantQuery = productVariantQuery;
        LazyContext = lazyContext;
    }

    public Product Product { get; set; }
    public ProductVariantQuery ProductVariantQuery { get; set; }
    public ProductLazyContext LazyContext { get; set; }
     
}


 