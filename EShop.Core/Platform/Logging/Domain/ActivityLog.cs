using EShop.Infrastructure.Domain;

namespace EShop.Core.Platform.Logging.Domain;

public class ActivityLog : BaseEntity
{
    public int UserId { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public int ActivityLogTypeId { get; set; }
}