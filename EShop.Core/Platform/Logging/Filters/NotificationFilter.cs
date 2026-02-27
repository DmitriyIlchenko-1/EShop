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

            var list = dictionary.TryGetValue(NotificationKey, out var value)
                ? JsonConvert.DeserializeObject<IList<NotificationEntry>>(value.ToString())
                : new List<NotificationEntry>();


            foreach (var newEntry in newEntries)
            {
                list.Add(newEntry);
            }

            dictionary[NotificationKey] = JsonConvert.SerializeObject(list);
        }
    }
}