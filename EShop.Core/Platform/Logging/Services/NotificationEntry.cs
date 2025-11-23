namespace EShop.Core.Platform.Logging.Services;

public enum NotificationType
{
    Information,
    Success,
    Warning,
    Error
}
public class NotificationEntry
{
    public NotificationType NotificationType { get; set; }
    public string Message { get; set; }
    public bool Preserve { get; set; }
}