using Microsoft.AspNetCore.Http;

namespace EShop.Core.Platform.Logging.Services;

public interface INotificationManager
{
    void Add(NotificationType type, string msg, bool preserve);
    ICollection<NotificationEntry> Notifications { get; }
}

public class NotificationManager : INotificationManager
{
    private readonly HashSet<NotificationEntry> _notifications = new();

    public void Add(NotificationType type, string msg, bool preserve = true)
    {
        _notifications.Add(new NotificationEntry()
        {
            NotificationType = type,
            Message = msg,
            Preserve = preserve
        });
    }


    public ICollection<NotificationEntry> Notifications => this._notifications;
}