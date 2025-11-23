using EShop.Infrastructure.Domain;

namespace EShop.Core.Platform.Logging.Domain;

public class ActivityLogType : BaseEntity   
{
    public string SystemKeyword { get; set; }
    public string Name { get; set; }
    public bool Enabled { get; set; }
}