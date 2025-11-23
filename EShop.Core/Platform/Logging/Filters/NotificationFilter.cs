using EShop.Core.Platform.Logging.Services;
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

    class NotificationFilter : IResultFilter
    {
        private readonly INotificationManager _notificationManager;

        public NotificationFilter(INotificationManager notificationManager)
        {
            _notificationManager = notificationManager;
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (_notificationManager.Notifications.Count == 0)
                return;

            if (context.Controller is Controller controller)
            {
                Persist(controller.ViewData,
                    _notificationManager.Notifications.Where(x => x.Preserve == false));
                Persist(controller.TempData, _notificationManager.Notifications.Where(x => x.Preserve));
            }

            _notificationManager.Notifications.Clear();
        }

        protected virtual void Persist(IDictionary<string, object> dictionary,
            IEnumerable<NotificationEntry> newEntries)
        {
            if (!newEntries.Any())
                return;

            var list = dictionary.TryGetValue(NotificationKey, out var value)
                ? JsonConvert.DeserializeObject<IList<NotificationEntry>>(value.ToString())
                : new List<NotificationEntry>();


            foreach (var newEntry in newEntries)
            {
                list.Add(newEntry);
            }

            dictionary[NotificationKey] = JsonConvert.SerializeObject(list);
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}