 
using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Platform.Caching;

public class DistributedCacheSettings : ISettings
{
    public string Endpoint { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string InstanceName { get; set; }
}