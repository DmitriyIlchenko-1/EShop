using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Routing;

namespace EShop.Infrastructure.Extensions;

public static class RouteExtensions
{
    private readonly static CompositeFormat ParsedRouteFormat = CompositeFormat.Parse("{0}{1}.{2}");


    public static string GetRouteString(this RouteValueDictionary routeValues)
    {
        var area = routeValues.GetValueOrDefaultAs<string, object, string>("area");
        var controller = routeValues.GetValueOrDefaultAs<string, object, string>("controller");
        var action = routeValues.GetValueOrDefaultAs<string, object, string>("action");

        return string.Format(CultureInfo.InvariantCulture,
            ParsedRouteFormat,
            area.HasValue() ? area + '.' : string.Empty,
            controller,
            action);
    }
}