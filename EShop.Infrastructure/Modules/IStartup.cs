using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Infrastructure.Modules;

public interface IEStartup
{
    int Order { get; }
    void ConfigureApplication(IApplicationBuilder app);
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}