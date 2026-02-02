using Autofac;
using EShop.Core.Catalog.Attributes.Modeling;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Common.Services;
using EShop.Core.Content.Media.Services;
using EShop.Core.Content.Widgets.Services;
using EShop.Core.Data;
using EShop.Core.Data.DbHandlers;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Configuration.Domain;
using EShop.Core.Platform.Configuration.Services;
using EShop.Core.Platform.Identity.Services;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Core.Platform.Logging.Services;
using EShop.Core.Platform.Routing;
using EShop.Core.Platform.Web;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Infrastructure;

public class CoreStartup : BaseStartup
{
    
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IJsonSerializer, NewtonsoftJsonSerializer>();
        services.AddScoped<IWidgetInstanceService, WidgetInstanceService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddSingleton<IProductPricingService, ProductPricingService>();
        services.AddSingleton<ICurrencyService, CurrencyService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddScoped<IActivityLogger, ActivityLogger>();
        services.AddScoped<IRecentlyViewedProductsService, RecentlyViewedProductsService>();
        services.AddScoped<IWorkContext, DefaultWorkContext>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWebHelper, DefaultWebHelper>();
        services.AddScoped<ICurrentUserWorkContextSource, CurrentUserWorkContextSource>();
        services.AddScoped<IProductAttributeMaterializer, ProductAttributeMaterializer>();
        services.AddScoped<IDeliveryTimeService, DeliveryTimeService>();
        services.AddScoped<ISettingFactory, SettingFactory>();
        services.AddScoped<INotificationManager, NotificationManager>();
        services.AddScoped<IUrlService, UrlService>();
        
        
        /*
         * Caching
         */
         services.AddCaching(configuration);
          

        services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration["DefaultConnection"],
                x => x.MigrationsAssembly("EShop.Web"));
        });


        services.AddScoped<IProductVariantQueryFactory, ProductVariantQueryFactory>();
 
        var settings = Singleton<ITypeScanner>.Instance.FindClassesOfType<ISettings>();
        foreach (var setting in settings)
        {
            services.AddScoped(setting,
                serviceProvider =>
                {
                    var factory = serviceProvider.GetRequiredService<ISettingFactory>();
                    return factory.LoadSettingsAsync(setting)
                        .Result;
                });
        }


        services.AddScoped<SlugRouteValueTransformer>();
    }

    public override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule(new DbHandlerModule());
    }
}