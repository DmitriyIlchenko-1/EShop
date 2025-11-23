using System.Text.RegularExpressions;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;

namespace EShop.Core.Catalog.Attributes.Modeling;

public partial class ProductVariantQueryFactory : IProductVariantQueryFactory
{
    private static readonly Regex IsVariantKey = IsVariantKeyRegex();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private IReadOnlyDictionary<string, string> _queryItems;

    public ProductVariantQueryFactory(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ProductVariantQuery Current { get; private set; }

    private IReadOnlyDictionary<string, string> QueryItems
    {
        get
        {
            if (_queryItems != null)
            {
                return _queryItems;
            }

            var queryItems = new Dictionary<string, string>();
            HttpRequest? request = _httpContextAccessor?.HttpContext?.Request;
            if (request != null)
            {
                if (request.HasFormContentType)
                {
                    request
                        .Form.Where(x => !string.IsNullOrWhiteSpace(x.Key)
                                         && !string.IsNullOrWhiteSpace(x.Value))
                        .Each(x => queryItems.Add(x.Key, x.Value));
                }

                request
                    .Query.Where(x => !string.IsNullOrWhiteSpace(x.Key)
                                      && !string.IsNullOrWhiteSpace(x.Value))
                    .Each(x => queryItems.Add(x.Key, x.Value));
            }

            _queryItems = queryItems.AsReadOnly();

            return _queryItems;
        }
    }

    public ProductVariantQuery CreateFromQuery()
    {
        ProductVariantQuery productVariantQuery = new ProductVariantQuery();
        var httpRequest = _httpContextAccessor?.HttpContext?.Request;
        if (httpRequest is null)
        {
            return productVariantQuery;
        }

        foreach (var queryItem in QueryItems)
        {
            if (queryItem.Value == null)
            {
                continue;
            }

            if (IsVariantKey.IsMatch(queryItem.Key))
            {
                CreateVariant(productVariantQuery, queryItem.Key, queryItem.Value);
            }
        }


        return Current = productVariantQuery;
    }

    private void CreateVariant(ProductVariantQuery query, string key, string value)
    {
        var ids = key
            .Replace("pvar", string.Empty)
            .Split('-')
            .ToArray();
        if (ids.Length < 4)
        {
            return;
        }

        query.AddVariant(new ProductVariantQueryItem()
        {
            Value = value,
            ProductId = ids[0]
                .ToInt32(),
            AttributeId = ids[1]
                .ToInt32(),
            VariantAttributeId = ids[2]
                .ToInt32()
        });
    }


    [GeneratedRegex("pvar[0-9]+-[0-9]+-[0-9]+-[0-9]+", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex IsVariantKeyRegex();
}