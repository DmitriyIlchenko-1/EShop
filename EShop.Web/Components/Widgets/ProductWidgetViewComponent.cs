// using EShop.Core.Catalog.Products.Domain;
// using EShop.Core.Catalog.Products.Services;
// using EShop.Core.Common.Services;
// using EShop.Core.Data;
// using EShop.Web.Models.Catalog;
// using EShop.Web.Models.Widgets;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Newtonsoft.Json;
//
// namespace EShop.Web.Components.Widgets;
//
// public class
//     ProductWidgetViewComponent : ViewComponent
// {
//     private readonly ApplicationDbContext _db;
//     private readonly IDateTimeService _timeService;
//     private readonly IMediaService _mediaService;
//     private readonly IProductPricingService _productPricingService;
//
//     public ProductWidgetViewComponent(IMediaService mediaService,
//         IProductPricingService productPricingService, ApplicationDbContext db, IDateTimeService timeService)
//     {
//         _mediaService = mediaService;
//         _productPricingService = productPricingService;
//         _db = db;
//         _timeService = timeService;
//     }
//
//     public async Task<IViewComponentResult> InvokeAsync(WidgetInstanceModel widgetInstance)
//     {
//         var model = new ProductWidgetComponentModel()
//         {
//             Id = widgetInstance.Id,
//             WidgetName = widgetInstance.Name,
//             Setting = JsonConvert.DeserializeObject<ProductWidgetSetting>(widgetInstance.Data)
//         };
//         IQueryable<Product> query = _db.Products
//             .Where(x => x.IsPublished && x.IsVisibleIndividually);
//
//         if (model.Setting.CategoryId.HasValue && model.Setting.CategoryId.Value > 0)
//         {
//             query = query.Where(x => x.ProductCategories.Any(c => c.CategoryId == model.Setting.CategoryId.Value));
//         }
//
//         if (model.Setting.FeaturedOnly)
//         {
//             query = query.Where(x => x.IsFeatured);
//         }
//
//         List<ProductThumbnail> productThumbnails = await query
//             .OrderByDescending(x => x.)
//             .Take(model.Setting.NumberOfProducts)
//             .Select(product => new ProductThumbnail()
//             {
//                 Id = product.Id,
//                 Name = product.Name,
//                 Slug = product.Slug,
//                 Price = product.Price,
//                 OldPrice = product.OldPrice,
//                 SpecialPrice = product.SpecialPrice,
//                 SpecialPriceStarts = product.SpecialPriceStartsUtc.HasValue
//                     ? _timeService.ConvertToLocalTimeZoneFromUtc(product.SpecialPriceStartsUtc.Value)
//                     : product.SpecialPriceStartsUtc,
//                 SpecialPriceEnds = product.SpecialPriceEndsUtc.HasValue
//                     ? _timeService.ConvertToLocalTimeZoneFromUtc(product.SpecialPriceEndsUtc.Value)
//                     : product.SpecialPriceEndsUtc,
//                 HasOptions = product.HasOptions,
//                 IsVisibleIndividually = product.IsVisibleIndividually,
//                 IsAllowToOrder = product.IsAllowToOrder,
//                 StockQuantity = product.StockQuantity,
//                 ReviewsCount = (int?)product.ReviewsCount,
//                 RatingAverage = product.RatingAverage,
//                 ThumbnailImage = product.ThumbnailImage
//             })
//             .ToListAsync();
//
//         model.Products = productThumbnails;
//
//         foreach (ProductThumbnail productThumbnail in productThumbnails)
//         {
//             productThumbnail.ThumbnailUrl = _mediaService.GetMediaUrl(productThumbnail.ThumbnailImage);
//             productThumbnail.CalculatedProductPrice = _productPricingService.CalculateProductPrice(
//                 productThumbnail.Price,
//                 productThumbnail.OldPrice,
//                 productThumbnail.SpecialPrice,
//                 productThumbnail.SpecialPriceStarts,
//                 productThumbnail.SpecialPriceEnds);
//         }
//
//
//         return View(model);
//     }
// }