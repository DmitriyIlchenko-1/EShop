using EShop.Core.Data;
using EShop.Core.Platform.Routing.Domain;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Platform.Routing
{
    public class SlugRouteValueTransformer : DynamicRouteValueTransformer
    {
        private readonly ApplicationDbContext _db;

        public SlugRouteValueTransformer(ApplicationDbContext db)
        {
            _db = db;
        }


        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext,
            RouteValueDictionary values)
        {
            string requestPath = httpContext.Request.Path.Value;

            if (string.IsNullOrEmpty(requestPath))
            {
                return null;
            }

            if (requestPath[0] == '/')
            {
                requestPath = requestPath.Substring(1);
            }

            UrlRecord? urlRecord = await _db
                .UrlRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Slug == requestPath);

            if (urlRecord is null)
            {
                return null;
            }

            var transformedValues = GetRouteValues(urlRecord, values);
            if (transformedValues == null)
            {
                return null;
            }

            return transformedValues;
        }

        protected virtual RouteValueDictionary GetRouteValues(UrlRecord urlRecord, RouteValueDictionary values)
        {
            switch (urlRecord.EntityName.ToLowerInvariant())
            {
                case "product":
                    return new RouteValueDictionary
                    {
                        { "area", string.Empty },
                        { "controller", "Product" },
                        { "action", "ProductDetails" },
                        { "productId", urlRecord.EntityId },
                    };
            }

            return null;
        }
    }

    public static class SlugRouteValueTransformerExtensions
    {
        public static IEndpointRouteBuilder MapSlugRouteValues(this IEndpointRouteBuilder builder)
        {
            Guard.NotNull(builder);
            builder.MapControllerRoute("Product",
                "{**SeName:minlength(2)}",
                new { controller = "Product", action = "ProductDetails" })
                .WithMetadata(new SuppressMatchingMetadata());
            builder.MapControllerRoute("Category",
                "{**SeName:minlength(2)}",
                new { controller = "Category", action = "Category" })
                .WithMetadata(new SuppressMatchingMetadata());;
            return builder;
        }
    }
}

 