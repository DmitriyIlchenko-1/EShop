using Autofac;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Media.Images;
using EShop.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace EShop.Web.Common.Infrustructure;

public class StaticFilesStartup : BaseStartup
{
    public override int Order => PipelineOrder.StaticFilesMiddleware;

    public override void ConfigureApplication(IApplicationBuilder applicationBuilder)
    {
        var app = EngineContext.Current.ApplicationContext;
        
        applicationBuilder.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = app.ImageRoot,
            RequestPath = "/images",
        });
    }
}