using System.Text.Json;
using EShop.Core.Platform.Logging.Filters;
using EShop.Core.Platform.Logging.Services;
using EShop.Web.Common.Conponents;
using EShop.Web.Common.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace EShop.Web.Components;

public class NotificationViewComponent : BaseViewComponent
{
    private readonly IViewDataAccessor _viewDataAccessor;

    public NotificationViewComponent(IViewDataAccessor viewDataAccessor)
    {
        _viewDataAccessor = viewDataAccessor;
    }

    public IViewComponentResult Invoke()
    {
        NotificationViewModel model = new NotificationViewModel();
        var allNotifications = GetNotificationsCore();
        model.HasNotifications = allNotifications.Any();
        model.Errors = allNotifications.Where(x => x.NotificationType == NotificationType.Error).ToArray();
        model.Warnings = allNotifications.Where(x => x.NotificationType == NotificationType.Warning).ToArray();
        model.Successes = allNotifications.Where(x => x.NotificationType == NotificationType.Success).ToArray();
        return View(model);
    }

    private IEnumerable<NotificationEntry> GetNotificationsCore()
    {
        string key = NotificationFilterAttribute.NotificationKey;
        var tempData = HttpContext
            .RequestServices.GetRequiredService<ITempDataDictionaryFactory>()
            .GetTempData(HttpContext);
        IEnumerable<NotificationEntry> holder = Enumerable.Empty<NotificationEntry>();
        if (tempData.ContainsKey(key))
        {
            var notifications = JsonSerializer.Deserialize<NotificationEntry[]>(tempData[key]
                .ToString());
            holder = holder.Concat(notifications);
        }

        var viewData = _viewDataAccessor.ViewData;
        if (viewData.ContainsKey(key))
        {
            var notifications = JsonSerializer.Deserialize<NotificationEntry[]>(viewData[key]
                .ToString());
            holder = holder.Concat(notifications);
        }

        return holder.ToArray();
    }
}

public class NotificationViewModel
{
    public bool HasNotifications { get; internal set; }
    public IEnumerable<NotificationEntry> Errors { get; internal set; }
    public IEnumerable<NotificationEntry> Warnings { get; internal set; }
    public IEnumerable<NotificationEntry> Successes { get; internal set; }
 }