//
// using EShop.Web.Models.Common;
// using EShop.Web.Models.Widgets;
// using Microsoft.AspNetCore.Mvc;
// using Newtonsoft.Json;
//
// namespace EShop.Web.Components.Widgets;
//
// public class CarouselWidgetViewComponent : ViewComponent
// {
//     private readonly IMediaService _mediaService;
//
//     public CarouselWidgetViewComponent(IMediaService mediaService)
//     {
//         _mediaService = mediaService;
//     }
//
//     public async Task<IViewComponentResult> InvokeAsync(WidgetInstanceModel widgetInstance)
//     {
//         ArgumentNullException.ThrowIfNull(widgetInstance);
//         
//         CarouselWidgetViewComponentModel model = new CarouselWidgetViewComponentModel
//         {
//             Id = widgetInstance.Id,
//             Items = JsonConvert.DeserializeObject<ICollection<CarouselWidgetViewComponentItemModel>>(
//                 widgetInstance.Data) ?? new List<CarouselWidgetViewComponentItemModel>()
//         };
//
//         foreach (var itemVm in model.Items)
//         {
//             itemVm.Image = _mediaService.GetMediaUrl(itemVm.Image);
//         }
//
//         return View(model);
//         
//     }
// }