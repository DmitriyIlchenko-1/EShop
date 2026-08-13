 
using System.Text.Json;
using EShop.Infrastructure.Collections;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;

namespace EShop.Core.Catalog.Attributes.Domain;

public class ProductVariantAttributeSelection : IEquatable<ProductVariantAttributeSelection>
{
    private readonly MultiMap<int, int> _attributes = new();
    private string _jsonAttributeData;
    public bool _isJson;
    

    public IEnumerable<KeyValuePair<int, IEnumerable<int>>> Attributes
        => _attributes.AsEnumerable();

    public ProductVariantAttributeSelection(string rawAttributes)
    {
        _attributes = GetAttributesFromJson(rawAttributes);
    }

    public ProductVariantAttributeSelection()
    {
        
    }

    private MultiMap<int, int> GetAttributesFromJson(string rawAttributes)
    {
        if (rawAttributes.IsEmpty())
        {
            return new MultiMap<int, int>();
        }
        
        return JsonSerializer.Deserialize<MultiMap<int, int>>(rawAttributes);
    }
    
    public void AddAttribute(int attributeId, int value)
    {
        _attributes.Add(attributeId, value);
    }
    public void AddAttribute(int attributeId, IEnumerable<int> values)
    {
        _attributes.AddRange(attributeId, values);
    }
    
    public bool IsEmpty() => _attributes.Count == 0;

    public override bool Equals(object other)
        => Equals(other as ProductVariantAttributeSelection);

    public override int GetHashCode()
    {
        var combiner = HashCodeCombiner.Start();
        var attributes = Attributes.OrderBy(k => k.Key);

        foreach (var pair in attributes)
        {
            combiner.Add(pair.Key);

            foreach (var value in pair.Value.Select(x => x.ToString()).OrderBy(x => x))
            {
                combiner.Add(value);
            }
        }

        return combiner.GetCombinedHash();
    }

   
    public virtual string AsJson()
    {
        if (_jsonAttributeData.HasValue() && _isJson)
            return _jsonAttributeData;
        if (_attributes.Count == 0)
            return null;
        
        var json = JsonSerializer.Serialize(_attributes);
        _isJson = true;
        return _jsonAttributeData = json;
    }

    public static bool operator ==(ProductVariantAttributeSelection left, ProductVariantAttributeSelection right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ProductVariantAttributeSelection left, ProductVariantAttributeSelection right)
    {
        return !Equals(left, right);
    }

    public bool Equals(ProductVariantAttributeSelection? other)
    {
        if (other == null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        var selections1 = _attributes;
        var selections2 = other._attributes;

        if (selections1.Count != selections2.Count)
        {
            return false;
        }

        foreach (var select1 in selections1)
        {
            if (!selections2.ContainsKey(select1.Key))
            {
                return false;
            }

            var value1 = select1.Value;
            var value2 = selections2[select1.Key];

            var result = value1.Equals(value2);
            if (!result)
            {
                return false;
            }
        }

        return true;
    }
    
   
}

public static class ProductVariantAttributeSelectionExtensions
{
    public static bool IsNullOrEmpty(this ProductVariantAttributeSelection? attributeSelection)
    {
        return attributeSelection == null || attributeSelection.IsEmpty();
    }
}