using EShop.Core.Catalog.Attributes.Modeling;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Core.Catalog.Attributes.Domain;

[ModelBinder(typeof(ProductAttributeQueryModelBinder))]
public class ProductVariantQuery
{
    private readonly List<ProductVariantQueryItem> _variants = [];

    public IReadOnlyList<ProductVariantQueryItem> Variants => _variants;

    public void AddVariant(ProductVariantQueryItem item)
    {
        bool present = _variants.Any(x =>
            x.ProductId == item.ProductId &&
            x.AttributeId == item.AttributeId &&
            x.VariantAttributeId == item.VariantAttributeId &&
            x.Value == item.Value);
        if (!present)
        {
            _variants.Add(item);
        }
    }
}