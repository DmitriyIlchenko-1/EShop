using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Web.Common.Infrustructure;

public class ExceptionHandlerStartup : BaseStartup
{
    public override int Order => PipelineOrder.ExceptionHandlerMiddleware;

    public override void ConfigureApplication(IApplicationBuilder app)
    {
        IHostEnvironment environment = EngineContext.Current.Environment;
      
        if (!environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStatusCodePagesWithReExecute("/Error/{0}");

    }

    
}