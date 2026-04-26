using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Catalog.Categories.Domain
{
    public class ProductCategory : BaseEntity
    {
        public Category Category { get; set; }

        public int CategoryId { get; set; }

        public int DisplayOrder { get; set; }

        public Product Product { get; set; }

        public int ProductId { get; set; }
    }
}