using EShop.Core.Platform.Logging.Services;

namespace EShop.Core.Data.Extensions;

public static class NotificationExtension
{
    public static void AddSuccess(this INotificationManager manager, string message, bool durable = true)
    {
        manager.Add(NotificationType.Success, message, durable);
    }

    public static void AddInfo(this INotificationManager manager, string message, bool durable = true)
    {
        manager.Add(NotificationType.Information, message, durable);
    }

    public static void AddError(this INotificationManager manager, string message, bool durable = true)
    {
        manager.Add(NotificationType.Error, message, durable);
    }

    public static void AddWarning(this INotificationManager manager, string message, bool durable = true)
    {
        manager.Add(NotificationType.Warning, message, durable);
    }
}