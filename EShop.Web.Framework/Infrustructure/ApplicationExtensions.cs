using EShop.Infrastructure.Engine;
using Microsoft.AspNetCore.Builder;

namespace EShop.Web.Common.Infrustructure;

public static class ApplicationExtensions
{
    public static void ConfigureApplicationPipeline(this IApplicationBuilder applicationBuilder)
    {
       EngineContext.Current.ConfigureRequestPipeline(applicationBuilder);
    }
}