using EShop.Core.Platform.Logging.Services;
using EShop.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace EShop.Core.Platform.Logging.Filters;

public class NotificationFilterAttribute : TypeFilterAttribute 
{
    public const string NotificationKey = "eShop.Notifications";

    public NotificationFilterAttribute() : base(typeof(NotificationFilter))
    {
    }

    class NotificationFilter : IAsyncResultFilter
    {
        private readonly INotificationManager _notificationManager;

        public NotificationFilter(INotificationManager notificationManager)
        {
            _notificationManager = notificationManager;
        }


        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (_notificationManager.Notifications.Count == 0)
            {
                await next();
                return;
            }


            if (context.Controller is Controller controller)
            {
                Persist(controller.ViewData,
                    _notificationManager.Notifications.Where(x => x.Preserve == false));
                Persist(controller.TempData, _notificationManager.Notifications.Where(x => x.Preserve));
            }

            _notificationManager.Notifications.Clear();

            await next();
        }

        protected virtual void Persist(IDictionary<string, object> dictionary,
            IEnumerable<NotificationEntry> newEntries)
        {
            if (!newEntries.Any())
                return;

            var notifications = dictionary.TryGetValue(NotificationKey, out var value)
                ? JsonConvert.DeserializeObject<NotificationEntry[]>(value.ToString())
                : Array.Empty<NotificationEntry>();


            var persistSet = notifications.Union(newEntries.Where(x => x.Message.HasValue())).ToArray();

            if (persistSet.Any())
            {
                dictionary[NotificationKey] = JsonConvert.SerializeObject(TrimNotificationSize(persistSet));
            }
        }

        private NotificationEntry[] TrimNotificationSize(NotificationEntry[] notifications)
        {
            if (notifications.Length <= 5)
            {
                return notifications;
            }

            return notifications.Skip(notifications.Length - 5).Take(5).ToArray();
        }
    }
}