using Autofac;
using EShop.Core.Catalog.Attributes.Modeling;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Checkout.Orders.Services;
using EShop.Core.Common.Services;
using EShop.Core.Content.Media.Services;
using EShop.Core.Content.Widgets.Services;
using EShop.Core.Data;
using EShop.Core.Data.Brands.Services;
using EShop.Core.Data.Cart.Services;
using EShop.Core.Data.Categories.Services;
using EShop.Core.Data.DbHandlers;
using EShop.Core.Data.Launch;
using EShop.Core.Data.Payment.Services;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Configuration.Domain;
using EShop.Core.Platform.Configuration.Services;
using EShop.Core.Platform.Identity.Services;
using EShop.Core.Platform.Infructructure.Types;
using EShop.Core.Platform.Logging.Services;
using EShop.Core.Platform.Routing;
using EShop.Core.Platform.Themes;
using EShop.Core.Platform.Themes.Services;
using EShop.Core.Platform.Web;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Common;
using EShop.Infrastructure.Email;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Media;
using EShop.Infrastructure.Media.Images;
using EShop.Infrastructure.Modules;
using EShop.Infrastructure.Modules.Launch;
using EShop.Infrastructure.Storage;
using EShop.Infrastructure.Utilities;
using EShop.Web.Common.Razor;
using EShop.Web.Common.TagHelpers;
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
        services.AddScoped<IMediaStorageProvider, FileMediaStorageProvider>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddSingleton<ICurrencyService, CurrencyService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, DefaultCategoryService>();
        services.AddScoped<IBrandService, DefaultBrandService>();
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
        services.AddSingleton<IThemeContext, DefaultThemeContext>();
        services.AddSingleton<IThemeRegistry, DefaultThemeRegistry>();
        services.AddScoped<IThemeVariableService, DefaultThemeVariableService>();
        services.AddScoped<IViewHelper, DefaultViewHelper>();
        services.AddScoped<IRequestCache, DefaultRequestCache>();
        services.AddScoped<IProductPriceService, DefaultProductPriceService>();
        services.AddScoped<IPriceCalculatorFactory, DefaultPriceCalculatorFactory>();
        services.AddScoped<IDiscountService, DefaultDiscountService>();
        services.AddSingleton<ILabelManager, DefaultLabelManager>();
        services.AddScoped<IEmailService, DefaultEmailService>();
        services.AddScoped<IViewRenderer<PartialViewRendererDescriptor>, PartialViewRenderer>();
        services.AddScoped<IViewRenderer<ComponentViewRendererDescriptor>, ComponentViewRenderer>();
        services.AddScoped<IViewRendererFactory, DefaultViewRendererFactory>();
        services.AddScoped<IShoppingCartService, DefaultShoppingCartService>();
        services.AddScoped<IOrderService, DefaultOrderService>();
        services.AddScoped<IAddressService, DefaultAddressService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<IPaymentService, PaymentService>();
         
        /*
         * Caching 
         */
         services.AddCaching(configuration);
          

        services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration["DbConnections:DefaultDbConnection"],
                x => x.MigrationsAssembly("EShop.Web")).EnableSensitiveDataLogging();
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

    public override void ConfigureContainer(ContainerBuilder builder, IApplicationContext applicationContext)
    {
        builder.RegisterModule(new ModuleDiscoveryModule(applicationContext));
        builder.RegisterModule(new DbHandlerModule());
        builder.RegisterModule(new PriceModule());
        builder.RegisterModule(new MediaModule());
    }
}