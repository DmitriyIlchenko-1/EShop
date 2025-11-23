using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Web.Common.Infrustructure;

public class ExceptionHandlerStartup : IEStartup
{
    public int Order => PipelineOrder.ExceptionHandlerMiddleware;

    public void ConfigureApplication(IApplicationBuilder app)
    {
        IWebHostEnvironment environment = EngineContext.Current.Resolve<IWebHostEnvironment>();
        if (environment.IsDevelopment())
            app.UseDeveloperExceptionPage();
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}