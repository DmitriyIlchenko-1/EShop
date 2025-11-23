using EShop.Core.Catalog.Attributes.Domain;

namespace EShop.Core.Catalog.Attributes.Modeling;

public interface IProductVariantQueryFactory
{
    public ProductVariantQuery Current { get; }
    public ProductVariantQuery CreateFromQuery();

}