using EShop.Core.Data;
using EShop.Core.Platform.Routing.Domain;
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
                .Include(x => x.EntityType)
                .FirstOrDefaultAsync(x => x.Slug == requestPath);

            if (urlRecord is null)
            {
                return null;
            }

            return new RouteValueDictionary
            {
                { "area", urlRecord.EntityType.TargetAreaName },
                { "controller", urlRecord.EntityType.TargetControllerName },
                { "action", urlRecord.EntityType.TargetActionName },
                { "id", urlRecord.EntityId }
            };
        }
    }
}